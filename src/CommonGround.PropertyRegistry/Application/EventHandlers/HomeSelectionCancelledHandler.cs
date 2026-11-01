using CommonGround.HomeBuyers.Contracts.Events;
using CommonGround.PropertyRegistry.Domain.Homes;
using CommonGround.PropertyRegistry.Domain.Homes.Events;
using CommonGround.SharedKernel.Extensions;
using JasperFx.Events;
using Marten;

namespace CommonGround.PropertyRegistry.Application.EventHandlers;

/// <summary>Makes a home available again when its selection is cancelled without choosing anyone.</summary>
public static class HomeSelectionCancelledHandler
{
    /// <summary>Fetches the home's stream for writing.</summary>
    public static Task<IEventStream<Home>> LoadAsync(HomeSelectionCancelled @event, IDocumentSession session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(session);

        return session.Events.FetchForWriting<Home>(@event.HomeId, cancellationToken);
    }

    /// <summary>Closes selection on the home, if it is still in selection.</summary>
    public static void Handle(HomeSelectionCancelled @event, IEventStream<Home> stream)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(stream);

        stream.Decide(home =>
        {
            if (home.Status != HomeStatus.InSelection)
                return null;

            return home.CloseSelectionWithoutSale(@event.HomeSelectionId);
        });
    }
}
