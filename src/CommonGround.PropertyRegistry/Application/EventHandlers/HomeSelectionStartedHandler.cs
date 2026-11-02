using CommonGround.HomeBuyers.Contracts.Events;
using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.SharedKernel.Domain.Services;
using Marten;
using Wolverine;
using Wolverine.Marten;

namespace CommonGround.PropertyRegistry.Application.EventHandlers;

/// <summary>Shows a home in selection when HomeBuyers starts choosing a household for it.</summary>
public static class HomeSelectionStartedHandler
{
    /// <summary>Loads the home's availability, and resolves the date the selection started in the trust's time zone.</summary>
    public static async Task<(HomeAvailabilityView?, DateOnly)> LoadAsync(
        HomeSelectionStarted @event,
        IDocumentSession session,
        ITrustClock trustClock,
        IMessageContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(trustClock);
        ArgumentNullException.ThrowIfNull(context);

        var availability = await HomeAvailabilityUpdate.LoadAsync(session, @event.HomeId, cancellationToken).ConfigureAwait(false);
        var openedDate = await trustClock.ToTrustDateAsync(context.TenantId!, @event.StartedAt, cancellationToken).ConfigureAwait(false);

        return (availability, openedDate);
    }

    /// <summary>Records the selection's start on the home's availability.</summary>
    public static IMartenOp Handle(HomeSelectionStarted @event, HomeAvailabilityView? availability, DateOnly openedDate)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return HomeAvailabilityUpdate.Record(@event.HomeId, availability,
            view => view.RecordSelectionStarted(@event.HomeSelectionId, @event.StartedAt, openedDate));
    }
}
