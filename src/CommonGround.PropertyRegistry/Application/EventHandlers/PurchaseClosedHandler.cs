using CommonGround.Homeownership.Contracts.Events;
using CommonGround.PropertyRegistry.Domain.Homes;
using CommonGround.PropertyRegistry.Domain.Homes.Events;
using CommonGround.SharedKernel.Extensions;
using JasperFx.Events;
using Marten;

namespace CommonGround.PropertyRegistry.Application.EventHandlers;

/// <summary>Marks a home leased when its purchase closes.</summary>
public static class PurchaseClosedHandler
{
    /// <summary>Fetches the home's stream for writing.</summary>
    public static Task<IEventStream<Home>> LoadAsync(PurchaseClosed @event, IDocumentSession session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(session);

        return session.Events.FetchForWriting<Home>(@event.HomeId, cancellationToken);
    }

    /// <summary>Marks the home leased from the closing date.</summary>
    public static void Handle(PurchaseClosed @event, IEventStream<Home> stream)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(stream);

        stream.Decide(home =>
        {
            return home.MarkLeased(@event.ClosingDate);
        });
    }
}
