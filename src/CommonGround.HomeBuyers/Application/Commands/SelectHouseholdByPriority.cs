using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.HomeBuyers.Application.Commands;

/// <summary>
/// Chooses a candidate in a home selection, citing the priorities that put them first, and
/// selects their application for the home at <paramref name="PurchasePrice"/>.
/// </summary>
public sealed record SelectHouseholdByPriority(
    HomeSelectionId SelectionId,
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
    /// <summary>Fetches the selection and its candidates' applications for writing.</summary>
    public static Task<SelectionCompletion> LoadAsync(
        SelectHouseholdByPriority command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        return HomeSelectionCompletion.LoadAsync(session, command.SelectionId, cancellationToken);
    }

    /// <summary>Records the choice, selects the chosen application, and passes over the others.</summary>
    public static void Handle(SelectHouseholdByPriority command, SelectionCompletion completion)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(completion);

        completion.Selection.Decide(selection =>
            selection.SelectByPriority(command.ApplicationId, command.Priorities, command.Reason, command.SelectedBy));

        HomeSelectionCompletion.Complete(
            completion,
            command.ApplicationId,
            command.GroundLeaseTemplateId,
            command.PurchasePrice,
            command.SelectedDate,
            command.SelectedBy);
    }
}
