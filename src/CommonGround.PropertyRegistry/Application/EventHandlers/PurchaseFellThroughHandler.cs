using CommonGround.Homeownership.Contracts.Events;
using CommonGround.PropertyRegistry.Domain.Homes;
using CommonGround.PropertyRegistry.Domain.Homes.Events;
using CommonGround.SharedKernel.Extensions;
using JasperFx.Events;
using Marten;

namespace CommonGround.PropertyRegistry.Application.EventHandlers;

/// <summary>Makes a home available again when its purchase fails or is withdrawn.</summary>
public static class PurchaseFellThroughHandler
{
    /// <summary>Fetches the home's stream for writing.</summary>
    public static Task<IEventStream<Home>> LoadAsync(PurchaseFellThrough @event, IDocumentSession session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(session);

        return session.Events.FetchForWriting<Home>(@event.HomeId, cancellationToken);
    }

    /// <summary>
    /// Releases the home's contract. A purchase started directly, rather than from a home selection,
    /// never put the home under contract, so there is nothing to release.
    /// </summary>
    public static void Handle(PurchaseFellThrough @event, IEventStream<Home> stream)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(stream);

        stream.Decide(home =>
        {
            if (home.Status != HomeStatus.UnderContract)
                return null;

            return home.ReleaseContract(@event.FellThroughDate);
        });
    }
}
