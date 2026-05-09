using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeaseTemplates;
using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.Homeownership.Application.EventHandlers;

/// <summary>
/// Applies purchase-closing outcomes to the linked ground lease.
/// On close, the workflow buyer becomes the lease homeowner.
/// </summary>
public sealed class PurchaseClosedHandler(IDocumentSession session)
{
    /// <summary>
    /// Handles purchase closure by onboarding or changing the ground lease homeowner.
    /// </summary>
    /// <param name="event">The purchase closed domain event.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Handle(PurchaseClosed @event, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var workflowId = PurchaseWorkflowId.Create(@event.AggregateId);

        var workflow = await session
            .LoadAggregateAsync<PurchaseWorkflow>(workflowId, ct)
            .ConfigureAwait(false);

        var groundLeaseTemplate = await session
            .LoadAggregateAsync<GroundLeaseTemplate>(workflow.GroundLeaseTemplateId, ct)
            .ConfigureAwait(false);

        var recordedBy = UserId.Create(@event.RecordedBy);

        var lease = GroundLease.CreateFromPurchase(
            groundLeaseTemplate,
            workflow,
            @event.ActualClosingDate,
            recordedBy
        );

        session.UpdateAggregate(lease);
    }
}
