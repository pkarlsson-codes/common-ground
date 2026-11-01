using CommonGround.Homeownership.Contracts.Events;
using CommonGround.PropertyRegistry.Domain.Homes;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.PropertyRegistry.Application.EventHandlers;

/// <summary>Marks a home leased when its purchase closes.</summary>
public static class PurchaseClosedHandler
{
    /// <summary>Marks the home leased from the closing date.</summary>
    public static async Task Handle(
        PurchaseClosed @event,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var home = await session.LoadAggregateAsync<Home>(HomeId.Create(@event.HomeId), cancellationToken).ConfigureAwait(false);
        home.MarkLeased(@event.ClosingDate);
        session.UpdateAggregate(home);
    }
}
