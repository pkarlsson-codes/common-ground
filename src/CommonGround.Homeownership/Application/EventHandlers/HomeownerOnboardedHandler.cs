using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeases.Events;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;
using Wolverine;
using GroundLeaseCreatedIntegration = CommonGround.Homeownership.Contracts.Events.GroundLeaseCreated;
using PurchaseClosedIntegration = CommonGround.Homeownership.Contracts.Events.PurchaseClosed;

namespace CommonGround.Homeownership.Application.EventHandlers;

/// <summary>
/// Publishes integration events when a homeowner is onboarded to a lease created by a purchase closing, so other
/// BCs can react: the lease exists, and the purchase it came from has closed.
/// </summary>
public sealed class HomeownerOnboardedHandler(IMessageBus bus, IQuerySession session)
{
    /// <summary>Handles the domain event and publishes the integration events to the bus.</summary>
    public async Task Handle(GroundLeaseCreatedFromPurchase @event, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(@event);

        await bus.PublishAsync(new GroundLeaseCreatedIntegration(
            @event.AggregateId,
            @event.BuyerFullName,
            @event.BuyerEmail,
            @event.BuyerPhone,
            @event.RegisteredBy))
            .ConfigureAwait(false);

        var workflow = await session.LoadAsync<PurchaseWorkflowView>(@event.PurchaseWorkflowId, ct).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(@event.PurchaseWorkflowId);

        await bus.PublishAsync(new PurchaseClosedIntegration(
            workflow.Id,
            workflow.HomeId,
            workflow.ApplicationId,
            workflow.HouseholdId,
            @event.LeaseStartDate,
            @event.RegisteredBy))
            .ConfigureAwait(false);
    }
}
