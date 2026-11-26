using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using Marten;

namespace CommonGround.HomeBuyers.Application.Commands;

/// <summary>
/// Chooses a household for a home from the waiting list as it is, citing the priorities that put it first, and
/// selects its application for the home at <paramref name="PurchasePrice"/>.
/// </summary>
/// <param name="SelectionId">The new selection's ID, chosen by the caller.</param>
/// <param name="HomeId">The home.</param>
/// <param name="ApplicationId">The chosen application.</param>
/// <param name="Priorities">The priorities that put the household first.</param>
/// <param name="Reason">Staff's explanation of the choice.</param>
/// <param name="GroundLeaseTemplateId">The ground lease template the purchase will use.</param>
/// <param name="PurchasePrice">The purchase price.</param>
/// <param name="SelectedDate">The date of the choice; the waiting list is judged on it, and it can't be after today.</param>
/// <param name="SelectedBy">The staff user choosing.</param>
public sealed record SelectHouseholdByPriority(
    HomeSelectionId SelectionId,
    string HomeId,
    HomeownerApplicationId ApplicationId,
    IReadOnlyList<SelectionPriority> Priorities,
    string Reason,
    string GroundLeaseTemplateId,
    decimal PurchasePrice,
    DateOnly SelectedDate,
    UserId SelectedBy);

/// <summary>Handles <see cref="SelectHouseholdByPriority"/>.</summary>
public static class SelectHouseholdByPriorityHandler
{
    /// <summary>Loads the home and compares the waiting list against it on the date of the choice.</summary>
    public static Task<HomeChoice> LoadAsync(
        SelectHouseholdByPriority command,
        IDocumentSession session,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        return HomeSelectionChoice.LoadAsync(session, trustClock, command.HomeId, command.SelectedDate, cancellationToken);
    }

    /// <summary>
    /// Records the choice with the whole waiting list, then selects the chosen application.
    /// The households passed over are recorded afterwards.
    /// </summary>
    public static Task Handle(
        SelectHouseholdByPriority command,
        HomeChoice choice,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(choice);

        HomeSelectionChoice.EnsureOffered(choice.Home, command.SelectedDate);

        var terms = new SelectionTerms(command.GroundLeaseTemplateId, command.PurchasePrice, command.SelectedDate);
        var chosen = HomeSelection.SelectByPriority(
            command.SelectionId,
            command.HomeId,
            choice.Comparison,
            command.ApplicationId,
            command.Priorities,
            command.Reason,
            terms,
            choice.Today,
            command.SelectedBy);

        return HomeSelectionChoice.RecordAsync(
            session, choice, command.SelectionId, chosen, command.ApplicationId, terms, command.SelectedBy, cancellationToken);
    }
}
