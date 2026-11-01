using CommonGround.Homeownership.Contracts.Events;
using CommonGround.PropertyRegistry.Domain.Homes;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.PropertyRegistry.Application.EventHandlers;

/// <summary>Makes a home available again when its purchase fails or is withdrawn.</summary>
public static class PurchaseFellThroughHandler
{
    /// <summary>
    /// Releases the home's contract. A purchase started directly, rather than from a home selection,
    /// never put the home under contract, so there is nothing to release.
    /// </summary>
    public static async Task Handle(
        PurchaseFellThrough @event,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var home = await session.LoadAggregateAsync<Home>(HomeId.Create(@event.HomeId), cancellationToken).ConfigureAwait(false);
        if (home.Status != HomeStatus.UnderContract)
            return;

        home.ReleaseContract(@event.FellThroughDate);
        session.UpdateAggregate(home);
    }
}
