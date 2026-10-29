using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.HomeBuyers.Application.Commands;

/// <summary>Chooses a candidate in a home selection, citing the priorities that put them first.</summary>
public sealed record SelectHouseholdByPriority(
    HomeSelectionId SelectionId,
    HomeownerApplicationId ApplicationId,
    IReadOnlyList<SelectionPriority> Priorities,
    string Reason,
    UserId SelectedBy);

/// <summary>Handles <see cref="SelectHouseholdByPriority"/>.</summary>
public static class SelectHouseholdByPriorityHandler
{
    /// <summary>Loads the selection, records the choice, and persists it.</summary>
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

        session.UpdateAggregate(selection);
    }
}
