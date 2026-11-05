using CommonGround.Homeownership.Contracts.Events;
using CommonGround.PropertyRegistry.Application.ReadModels;
using Marten;
using Wolverine.Marten;

namespace CommonGround.PropertyRegistry.Application.EventHandlers;

/// <summary>Shows a home leased when a ground lease is created for it, from a purchase closing or directly.</summary>
public static class GroundLeaseCreatedHandler
{
    /// <summary>Loads the home's availability.</summary>
    public static Task<HomeAvailabilityView?> LoadAsync(GroundLeaseCreated @event, IDocumentSession session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return HomeAvailabilityUpdate.LoadAsync(session, @event.HomeId, cancellationToken);
    }

    /// <summary>Records the lease on the home's availability.</summary>
    public static IMartenOp Handle(GroundLeaseCreated @event, HomeAvailabilityView? availability)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return HomeAvailabilityUpdate.Record(@event.HomeId, availability, view => view.RecordLeased(@event.LeaseStartDate));
    }
}
