using System.Security.Cryptography;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using Marten;

namespace CommonGround.HomeBuyers.Application.Commands;

/// <summary>
/// Chooses a household for a home by lottery from the waiting list as it is, drawing from
/// <paramref name="TiedApplicationIds"/> when given, otherwise from the whole waiting list, and selects the winner's
/// application for the home at <paramref name="PurchasePrice"/>.
/// </summary>
/// <param name="SelectionId">The new selection's ID, chosen by the caller.</param>
/// <param name="HomeId">The home.</param>
/// <param name="TiedApplicationIds">The applications staff found tied, or null to draw from the whole waiting list.</param>
/// <param name="GroundLeaseTemplateId">The ground lease template the purchase will use.</param>
/// <param name="PurchasePrice">The purchase price.</param>
/// <param name="SelectedDate">The date of the draw; the waiting list is judged on it, and it can't be after today.</param>
/// <param name="DrawnBy">The staff user drawing.</param>
public sealed record DrawSelectionLottery(
    HomeSelectionId SelectionId,
    string HomeId,
    IReadOnlyList<HomeownerApplicationId>? TiedApplicationIds,
    string GroundLeaseTemplateId,
    decimal PurchasePrice,
    DateOnly SelectedDate,
    UserId DrawnBy);

/// <summary>Handles <see cref="DrawSelectionLottery"/>.</summary>
public static class DrawSelectionLotteryHandler
{
    /// <summary>Loads the home and compares the waiting list against it on the date of the draw.</summary>
    public static Task<HomeChoice> LoadAsync(
        DrawSelectionLottery command,
        IDocumentSession session,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        return HomeSelectionChoice.LoadAsync(session, trustClock, command.HomeId, command.SelectedDate, cancellationToken);
    }

    /// <summary>
    /// Draws with a fresh random seed among the households that can be chosen, records the draw with the whole
    /// waiting list, then selects the winner's application. The households passed over are recorded afterwards.
    /// </summary>
    public static Task Handle(
        DrawSelectionLottery command,
        HomeChoice choice,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(choice);

        HomeSelectionChoice.EnsureOffered(choice.Home, command.SelectedDate);

        var terms = new SelectionTerms(command.GroundLeaseTemplateId, command.PurchasePrice, command.SelectedDate);
        var drawn = HomeSelection.DrawLottery(
            command.SelectionId,
            command.HomeId,
            choice.Comparison,
            command.TiedApplicationIds,
            RandomNumberGenerator.GetInt32(int.MaxValue),
            terms,
            choice.Today,
            command.DrawnBy);

        return HomeSelectionChoice.RecordAsync(
            session,
            choice,
            command.SelectionId,
            drawn,
            HomeownerApplicationId.Create(drawn.ApplicationId),
            terms,
            command.DrawnBy,
            cancellationToken);
    }
}
