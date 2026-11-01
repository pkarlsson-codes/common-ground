using CommonGround.HomeBuyers.Contracts.Events;
using CommonGround.PropertyRegistry.Domain.Homes;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.PropertyRegistry.Application.EventHandlers;

/// <summary>Moves a home into selection when HomeBuyers starts choosing a household for it.</summary>
public static class HomeSelectionStartedHandler
{
    /// <summary>
    /// Opens selection on the home. An under-contract home is one whose purchase fell through (HomeBuyers
    /// only starts a selection for it then), so it opens too; a leased home or one already in selection doesn't.
    /// </summary>
    public static async Task Handle(
        HomeSelectionStarted @event,
        IDocumentSession session,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var home = await session.LoadAggregateAsync<Home>(HomeId.Create(@event.HomeId), cancellationToken).ConfigureAwait(false);
        if (home.Status != HomeStatus.Available && home.Status != HomeStatus.UnderContract)
            return;

        var openedDate = await trustClock
            .ToTrustDateAsync(session.TenantId, @event.StartedAt, cancellationToken)
            .ConfigureAwait(false);
        home.OpenSelection(@event.HomeSelectionId, openedDate);
        session.UpdateAggregate(home);
    }
}
