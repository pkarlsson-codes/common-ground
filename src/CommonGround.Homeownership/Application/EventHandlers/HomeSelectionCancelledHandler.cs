using CommonGround.HomeBuyers.Contracts.Events;
using CommonGround.Homeownership.Application.ReadModels;
using Marten;
using Wolverine.Marten;

namespace CommonGround.Homeownership.Application.EventHandlers;

/// <summary>Records that a home's selection was cancelled, so the home can be leased by hand again.</summary>
public static class HomeSelectionCancelledHandler
{
    /// <summary>Loads the selection.</summary>
    public static Task<HomeSelectionView?> LoadAsync(HomeSelectionCancelled @event, IDocumentSession session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return HomeSelectionUpdate.LoadAsync(session, @event.HomeSelectionId, cancellationToken);
    }

    /// <summary>Records the selection's end.</summary>
    public static IMartenOp Handle(HomeSelectionCancelled @event, HomeSelectionView? selection)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return HomeSelectionUpdate.Record(@event.HomeSelectionId, @event.HomeId, selection, view => view.RecordEnded());
    }
}
