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
    /// <summary>Records the choice, selects the chosen application, passes over the others, and persists them.</summary>
    public static async Task Handle(
        SelectHouseholdByPriority command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var selection = await session
            .LoadAggregateAsync<HomeSelection>(command.SelectionId, cancellationToken)
            .ConfigureAwait(false);

        selection.SelectByPriority(command.ApplicationId, command.Priorities, command.Reason, command.SelectedBy);

        var candidates = await HomeSelectionCompletion
            .LoadCandidatesAsync(session, selection, cancellationToken)
            .ConfigureAwait(false);

        await HomeSelectionCompletion.CompleteAsync(
            session,
            selection,
            candidates,
            command.GroundLeaseTemplateId,
            command.PurchasePrice,
            command.SelectedDate,
            command.SelectedBy,
            cancellationToken).ConfigureAwait(false);

        session.UpdateAggregate(selection);
    }
}
