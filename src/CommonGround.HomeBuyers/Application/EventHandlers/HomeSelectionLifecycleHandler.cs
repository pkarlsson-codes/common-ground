using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;
using DomainHomeSelectionCancelled = CommonGround.HomeBuyers.Domain.HomeSelections.Events.HomeSelectionCancelled;
using DomainHomeSelectionStarted = CommonGround.HomeBuyers.Domain.HomeSelections.Events.HomeSelectionStarted;
using IntegrationHomeSelectionCancelled = CommonGround.HomeBuyers.Contracts.Events.HomeSelectionCancelled;
using IntegrationHomeSelectionStarted = CommonGround.HomeBuyers.Contracts.Events.HomeSelectionStarted;

namespace CommonGround.HomeBuyers.Application.EventHandlers;

/// <summary>Publishes when a home selection starts or is cancelled, so PropertyRegistry can track the home's stage.</summary>
public static class HomeSelectionLifecycleHandler
{
    /// <summary>Publishes <see cref="IntegrationHomeSelectionStarted"/>.</summary>
    public static IntegrationHomeSelectionStarted Handle(DomainHomeSelectionStarted @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return new IntegrationHomeSelectionStarted(@event.AggregateId, @event.HomeId, @event.OccurredAt);
    }

    /// <summary>Publishes <see cref="IntegrationHomeSelectionCancelled"/>.</summary>
    public static async Task<IntegrationHomeSelectionCancelled> Handle(
        DomainHomeSelectionCancelled @event,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(session);

        var selection = await session.LoadAsync<HomeSelectionView>(@event.AggregateId, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(@event.AggregateId);

        return new IntegrationHomeSelectionCancelled(@event.AggregateId, selection.HomeId);
    }
}
