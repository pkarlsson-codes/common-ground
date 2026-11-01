using CommonGround.HomeBuyers.Contracts.Events;
using CommonGround.PropertyRegistry.Domain.Homes;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.PropertyRegistry.Application.EventHandlers;

/// <summary>Makes a home available again when its selection is cancelled without choosing anyone.</summary>
public static class HomeSelectionCancelledHandler
{
    /// <summary>Closes selection on the home, if it is still in selection.</summary>
    public static async Task Handle(
        HomeSelectionCancelled @event,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var home = await session.LoadAggregateAsync<Home>(HomeId.Create(@event.HomeId), cancellationToken).ConfigureAwait(false);
        if (home.Status != HomeStatus.InSelection)
            return;

        home.CloseSelectionWithoutSale(@event.HomeSelectionId);
        session.UpdateAggregate(home);
    }
}
