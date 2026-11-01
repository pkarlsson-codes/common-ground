using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;
using IntegrationPurchaseFellThrough = CommonGround.Homeownership.Contracts.Events.PurchaseFellThrough;

namespace CommonGround.Homeownership.Application.EventHandlers;

/// <summary>
/// Publishes that a purchase fell through (failed or withdrawn), so PropertyRegistry can make the home available
/// again and HomeBuyers can return the household to the waiting list. A closed purchase is published once its
/// ground lease exists, by <see cref="HomeownerOnboardedHandler"/>.
/// </summary>
public static class PurchaseOutcomeHandler
{
    /// <summary>Publishes that a failed purchase fell through.</summary>
    public static async Task<IntegrationPurchaseFellThrough> Handle(PurchaseFailed @event, IQuerySession session, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(@event);
        var workflow = await LoadAsync(session, @event.AggregateId, ct).ConfigureAwait(false);
        return new IntegrationPurchaseFellThrough(
            workflow.Id, workflow.HomeId, workflow.ApplicationId, workflow.HouseholdId,
            @event.FailureReason, @event.FailedDate, @event.RecordedBy);
    }

    /// <summary>Publishes that a withdrawn purchase fell through.</summary>
    public static async Task<IntegrationPurchaseFellThrough> Handle(PurchaseWithdrawn @event, IQuerySession session, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(@event);
        var workflow = await LoadAsync(session, @event.AggregateId, ct).ConfigureAwait(false);
        return new IntegrationPurchaseFellThrough(
            workflow.Id, workflow.HomeId, workflow.ApplicationId, workflow.HouseholdId,
            @event.WithdrawalReason ?? "Withdrawn", @event.WithdrawnDate, @event.RecordedBy);
    }

    private static async Task<PurchaseWorkflowView> LoadAsync(IQuerySession session, string workflowId, CancellationToken ct) =>
        await session.LoadAsync<PurchaseWorkflowView>(workflowId, ct).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(workflowId);
}
