using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;
using Wolverine;
using IntegrationPurchaseFellThrough = CommonGround.Homeownership.Contracts.Events.PurchaseFellThrough;

namespace CommonGround.Homeownership.Application.EventHandlers;

/// <summary>
/// Publishes that a purchase fell through (failed or withdrawn), so PropertyRegistry can make the home available
/// again and HomeBuyers can return the household to the waiting list. A closed purchase is published once its
/// ground lease exists, by <see cref="HomeownerOnboardedHandler"/>.
/// </summary>
public sealed class PurchaseOutcomeHandler(IMessageBus bus, IQuerySession session)
{
    /// <summary>Publishes <see cref="IntegrationPurchaseFellThrough"/> for a failed purchase.</summary>
    public async Task Handle(PurchaseFailed @event, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(@event);
        var workflow = await LoadAsync(@event.AggregateId, ct).ConfigureAwait(false);
        await bus.PublishAsync(new IntegrationPurchaseFellThrough(
            workflow.Id, workflow.HomeId, workflow.ApplicationId, workflow.HouseholdId,
            @event.FailureReason, @event.FailedDate, @event.RecordedBy)).ConfigureAwait(false);
    }

    /// <summary>Publishes <see cref="IntegrationPurchaseFellThrough"/> for a withdrawn purchase.</summary>
    public async Task Handle(PurchaseWithdrawn @event, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(@event);
        var workflow = await LoadAsync(@event.AggregateId, ct).ConfigureAwait(false);
        await bus.PublishAsync(new IntegrationPurchaseFellThrough(
            workflow.Id, workflow.HomeId, workflow.ApplicationId, workflow.HouseholdId,
            @event.WithdrawalReason ?? "Withdrawn", @event.WithdrawnDate, @event.RecordedBy)).ConfigureAwait(false);
    }

    private async Task<PurchaseWorkflowView> LoadAsync(string workflowId, CancellationToken ct) =>
        await session.LoadAsync<PurchaseWorkflowView>(workflowId, ct).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(workflowId);
}
