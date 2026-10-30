using System.Security.Cryptography;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
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

/// <summary>Handles <see cref="DrawSelectionLottery"/>.</summary>
public static class DrawSelectionLotteryHandler
{
    /// <summary>
    /// Draws with a fresh random seed among the candidates who can be selected on the selected
    /// date, selects the winner's application, passes over the others, and persists them.
    /// </summary>
    public static async Task Handle(
        DrawSelectionLottery command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var selection = await session
            .LoadAggregateAsync<HomeSelection>(command.SelectionId, cancellationToken)
            .ConfigureAwait(false);

        var candidates = await HomeSelectionCompletion
            .LoadCandidatesAsync(session, selection, cancellationToken)
            .ConfigureAwait(false);

        var eligibleApplicationIds = candidates
            .Where(c => c.Application.CanBeSelected(c.Household, command.SelectedDate))
            .Select(c => c.Application.Id)
            .ToHashSet();

        var seed = RandomNumberGenerator.GetInt32(int.MaxValue);
        selection.DrawLottery(command.TiedApplicationIds, eligibleApplicationIds, seed, command.DrawnBy);

        await HomeSelectionCompletion.CompleteAsync(
            session,
            selection,
            candidates,
            command.GroundLeaseTemplateId,
            command.PurchasePrice,
            command.SelectedDate,
            command.DrawnBy,
            cancellationToken).ConfigureAwait(false);

        session.UpdateAggregate(selection);
    }
}
