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
    /// <summary>Fetches the selection and its candidates' applications for writing.</summary>
    public static Task<SelectionCompletion> LoadAsync(
        DrawSelectionLottery command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        return HomeSelectionCompletion.LoadAsync(session, command.SelectionId, cancellationToken);
    }

    /// <summary>
    /// Draws with a fresh random seed among the candidates who can be selected on the selected
    /// date, selects the winner's application, and passes over the others.
    /// </summary>
    public static void Handle(DrawSelectionLottery command, SelectionCompletion completion)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(completion);

        var eligibleApplicationIds = completion.Candidates
            .Where(c => c.Application.Aggregate!.CanBeSelected(c.Household, command.SelectedDate))
            .Select(c => c.Application.Aggregate!.Id)
            .ToHashSet();
        var seed = RandomNumberGenerator.GetInt32(int.MaxValue);

        var drawn = completion.CurrentSelection.DrawLottery(command.TiedApplicationIds, eligibleApplicationIds, seed, command.DrawnBy);
        completion.Selection.AppendOne(drawn);

        HomeSelectionCompletion.Complete(
            completion,
            HomeownerApplicationId.Create(drawn.ApplicationId),
            command.GroundLeaseTemplateId,
            command.PurchasePrice,
            command.SelectedDate,
            command.DrawnBy);
    }
}
