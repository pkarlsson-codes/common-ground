using System.Security.Cryptography;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using CommonGround.HomeBuyers.Domain.Households;
using JasperFx.Events;
using Marten;

namespace CommonGround.HomeBuyers.Application.Commands;

/// <summary>
/// Chooses a candidate in a home selection by lottery, drawing from
/// <paramref name="TiedApplicationIds"/> when given, otherwise from every candidate, and selects
/// the winner's application for the home at <paramref name="PurchasePrice"/>.
/// </summary>
public sealed record DrawSelectionLottery(
    HomeSelectionId SelectionId,
    IReadOnlyList<HomeownerApplicationId>? TiedApplicationIds,
    string GroundLeaseTemplateId,
    decimal PurchasePrice,
    DateOnly SelectedDate,
    UserId DrawnBy);

/// <summary>The selection, fetched for writing, and the candidates who can be selected on the selected date.</summary>
/// <param name="Selection">The selection's stream.</param>
/// <param name="EligibleApplicationIds">The candidates' applications that can be selected.</param>
public sealed record LotteryPool(
    IEventStream<HomeSelection> Selection,
    IReadOnlySet<HomeownerApplicationId> EligibleApplicationIds);

/// <summary>Handles <see cref="DrawSelectionLottery"/>.</summary>
public static class DrawSelectionLotteryHandler
{
    /// <summary>
    /// Fetches the selection for writing and reads, without locking, which candidates can be
    /// selected on the selected date. Only the winner's application is fetched for writing.
    /// </summary>
    public static async Task<LotteryPool> LoadAsync(
        DrawSelectionLottery command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(session);

        var selection = await HomeSelectionCompletion
            .FetchSelectionAsync(session, command.SelectionId, cancellationToken)
            .ConfigureAwait(false);

        var eligible = new HashSet<HomeownerApplicationId>();
        foreach (var candidate in selection.Aggregate!.Candidates)
        {
            var application = await session
                .LoadAggregateAsync<HomeownerApplication>(candidate.ApplicationId, cancellationToken)
                .ConfigureAwait(false);
            var household = await session
                .LoadAggregateAsync<Household>(candidate.HouseholdId, cancellationToken)
                .ConfigureAwait(false);
            if (application.CanBeSelected(household, command.SelectedDate))
            {
                eligible.Add(candidate.ApplicationId);
            }
        }

        return new LotteryPool(selection, eligible);
    }

    /// <summary>
    /// Draws with a fresh random seed among the eligible candidates, then fetches the winner's
    /// application for writing and selects it. The other candidates are passed over afterwards.
    /// </summary>
    public static async Task Handle(
        DrawSelectionLottery command,
        LotteryPool pool,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(pool);

        var current = pool.Selection.Aggregate!;
        var seed = RandomNumberGenerator.GetInt32(int.MaxValue);
        var drawn = current.DrawLottery(command.TiedApplicationIds, pool.EligibleApplicationIds, seed, command.DrawnBy);
        pool.Selection.AppendOne(drawn);

        var chosen = await HomeSelectionCompletion
            .FetchChosenAsync(session, current, HomeownerApplicationId.Create(drawn.ApplicationId), cancellationToken)
            .ConfigureAwait(false);
        HomeSelectionCompletion.Complete(
            current,
            chosen,
            command.GroundLeaseTemplateId,
            command.PurchasePrice,
            command.SelectedDate,
            command.DrawnBy);
    }
}
