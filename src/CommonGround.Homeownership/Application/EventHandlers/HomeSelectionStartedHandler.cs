using CommonGround.HomeBuyers.Contracts.Events;
using CommonGround.Homeownership.Application.ReadModels;
using Marten;
using Wolverine.Marten;

namespace CommonGround.Homeownership.Application.EventHandlers;

/// <summary>Records that a home is in selection, so it can't be leased by hand meanwhile.</summary>
public static class HomeSelectionStartedHandler
{
    /// <summary>Loads the selection.</summary>
    public static Task<HomeSelectionView?> LoadAsync(HomeSelectionStarted @event, IDocumentSession session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return HomeSelectionUpdate.LoadAsync(session, @event.HomeSelectionId, cancellationToken);
    }

    /// <summary>Records the selection's start.</summary>
    public static IMartenOp Handle(HomeSelectionStarted @event, HomeSelectionView? selection)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return HomeSelectionUpdate.Record(@event.HomeSelectionId, @event.HomeId, selection, view => view.RecordStarted());
    }
}
