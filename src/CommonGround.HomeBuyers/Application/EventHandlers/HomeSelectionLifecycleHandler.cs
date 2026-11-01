using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;
using Wolverine;
using DomainHomeSelectionCancelled = CommonGround.HomeBuyers.Domain.HomeSelections.Events.HomeSelectionCancelled;
using DomainHomeSelectionStarted = CommonGround.HomeBuyers.Domain.HomeSelections.Events.HomeSelectionStarted;
using IntegrationHomeSelectionCancelled = CommonGround.HomeBuyers.Contracts.Events.HomeSelectionCancelled;
using IntegrationHomeSelectionStarted = CommonGround.HomeBuyers.Contracts.Events.HomeSelectionStarted;

namespace CommonGround.HomeBuyers.Application.EventHandlers;

/// <summary>Publishes when a home selection starts or is cancelled, so PropertyRegistry can track the home's stage.</summary>
public sealed class HomeSelectionLifecycleHandler(IMessageBus bus, IQuerySession session)
{
    /// <summary>Publishes <see cref="IntegrationHomeSelectionStarted"/>.</summary>
    public async Task Handle(DomainHomeSelectionStarted @event, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);

        await bus.PublishAsync(new IntegrationHomeSelectionStarted(
            @event.AggregateId, @event.HomeId, @event.OccurredAt)).ConfigureAwait(false);
    }

    /// <summary>Publishes <see cref="IntegrationHomeSelectionCancelled"/>.</summary>
    public async Task Handle(DomainHomeSelectionCancelled @event, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var selection = await session.LoadAsync<HomeSelectionView>(@event.AggregateId, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(@event.AggregateId);

        await bus.PublishAsync(new IntegrationHomeSelectionCancelled(@event.AggregateId, selection.HomeId)).ConfigureAwait(false);
    }
}
