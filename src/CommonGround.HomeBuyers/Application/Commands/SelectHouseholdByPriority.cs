using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using JasperFx.Events;
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
    /// <summary>Fetches the selection for writing.</summary>
    public static Task<IEventStream<HomeSelection>> LoadAsync(
        SelectHouseholdByPriority command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        return HomeSelectionCompletion.FetchSelectionAsync(session, command.SelectionId, cancellationToken);
    }

    /// <summary>
    /// Records the choice, then fetches the chosen application for writing and selects it.
    /// The other candidates are passed over afterwards.
    /// </summary>
    public static async Task Handle(
        SelectHouseholdByPriority command,
        IEventStream<HomeSelection> selection,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(selection);

        var current = selection.Aggregate!;
        selection.Decide(s =>
            s.SelectByPriority(command.ApplicationId, command.Priorities, command.Reason, command.SelectedBy));

        var chosen = await HomeSelectionCompletion
            .FetchChosenAsync(session, current, command.ApplicationId, cancellationToken)
            .ConfigureAwait(false);
        HomeSelectionCompletion.Complete(
            current,
            chosen,
            command.GroundLeaseTemplateId,
            command.PurchasePrice,
            command.SelectedDate,
            command.SelectedBy);
    }
}
