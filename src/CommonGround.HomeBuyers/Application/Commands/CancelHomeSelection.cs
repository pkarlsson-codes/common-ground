using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using Marten;

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
    public static async Task Handle(
        CancelHomeSelection command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var selection = await session
            .LoadAggregateAsync<HomeSelection>(command.SelectionId, cancellationToken)
            .ConfigureAwait(false);

        selection.Cancel(command.Reason, command.CancelledBy);

        session.UpdateAggregate(selection);
    }
}
