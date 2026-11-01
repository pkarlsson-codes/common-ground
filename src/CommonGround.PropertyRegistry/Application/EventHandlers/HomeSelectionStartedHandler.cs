using CommonGround.HomeBuyers.Contracts.Events;
using CommonGround.PropertyRegistry.Domain.Homes;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using JasperFx.Events;
using Marten;
using Wolverine;

namespace CommonGround.PropertyRegistry.Application.EventHandlers;

/// <summary>Moves a home into selection when HomeBuyers starts choosing a household for it.</summary>
public static class HomeSelectionStartedHandler
{
    /// <summary>Fetches the home's stream for writing, and resolves the date the selection started in the trust's time zone.</summary>
    public static async Task<(IEventStream<Home>, DateOnly)> LoadAsync(
        HomeSelectionStarted @event,
        IDocumentSession session,
        ITrustClock trustClock,
        IMessageContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(trustClock);
        ArgumentNullException.ThrowIfNull(context);

        var stream = await session.Events.FetchForWriting<Home>(@event.HomeId, cancellationToken).ConfigureAwait(false);
        var openedDate = await trustClock.ToTrustDateAsync(context.TenantId!, @event.StartedAt, cancellationToken).ConfigureAwait(false);

        return (stream, openedDate);
    }

    /// <summary>
    /// Opens selection on the home. An under-contract home is one whose purchase fell through (HomeBuyers
    /// only starts a selection for it then), so it opens too; a leased home or one already in selection doesn't.
    /// </summary>
    public static void Handle(HomeSelectionStarted @event, IEventStream<Home> stream, DateOnly openedDate)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(stream);

        stream.Decide(home =>
        {
            if (home.Status != HomeStatus.Available && home.Status != HomeStatus.UnderContract)
                return null;

            return home.OpenSelection(@event.HomeSelectionId, openedDate);
        });
    }
}
