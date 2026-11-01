using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.HomeBuyers.Domain.HomeSelections.Events;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.HomeBuyers.Application.Commands;

/// <summary>Cancels an open home selection without choosing anyone.</summary>
public sealed record CancelHomeSelection(
    HomeSelectionId SelectionId,
    string Reason,
    UserId CancelledBy);

/// <summary>Handles <see cref="CancelHomeSelection"/>.</summary>
public static class CancelHomeSelectionHandler
{
    /// <summary>Loads the selection, cancels it, and persists it.</summary>
    public static HomeSelectionCancelled Handle(CancelHomeSelection command, [WriteAggregate(nameof(CancelHomeSelection.SelectionId))] HomeSelection selection)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(selection);

        return selection.Cancel(command.Reason, command.CancelledBy);
    }
}
