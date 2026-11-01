using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeases.Events;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;
using GroundLeaseCreatedIntegration = CommonGround.Homeownership.Contracts.Events.GroundLeaseCreated;
using PurchaseClosedIntegration = CommonGround.Homeownership.Contracts.Events.PurchaseClosed;

namespace CommonGround.Homeownership.Application.EventHandlers;

/// <summary>
/// Publishes integration events when a homeowner is onboarded to a lease created by a purchase closing, so other
/// BCs can react: the lease exists, and the purchase it came from has closed.
/// </summary>
public static class HomeownerOnboardedHandler
{
    /// <summary>Loads the purchase workflow the lease came from.</summary>
    public static async Task<PurchaseWorkflowView> LoadAsync(GroundLeaseCreatedFromPurchase @event, IQuerySession session, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(session);

        return await session.LoadAsync<PurchaseWorkflowView>(@event.PurchaseWorkflowId, ct).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(@event.PurchaseWorkflowId);
    }

    /// <summary>Publishes that the lease exists and that the purchase it came from has closed.</summary>
    public static (GroundLeaseCreatedIntegration, PurchaseClosedIntegration) Handle(
        GroundLeaseCreatedFromPurchase @event,
        PurchaseWorkflowView workflow)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(workflow);

        return (
            new GroundLeaseCreatedIntegration(
                @event.AggregateId,
                @event.BuyerFullName,
                @event.BuyerEmail,
                @event.BuyerPhone,
                @event.RegisteredBy),
            new PurchaseClosedIntegration(
                workflow.Id,
                workflow.HomeId,
                workflow.ApplicationId,
                workflow.HouseholdId,
                @event.LeaseStartDate,
                @event.RegisteredBy));
    }
}
