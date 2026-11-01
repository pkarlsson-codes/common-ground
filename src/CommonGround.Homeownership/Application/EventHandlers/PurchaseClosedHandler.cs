using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeaseTemplates;
using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using Marten;
using Wolverine.Marten;

namespace CommonGround.Homeownership.Application.EventHandlers;

/// <summary>
/// Applies purchase-closing outcomes to the linked ground lease.
/// On close, the workflow buyer becomes the lease homeowner.
/// </summary>
public static class PurchaseClosedHandler
{
    /// <summary>Loads the closed purchase workflow and the template its lease follows.</summary>
    public static async Task<(PurchaseWorkflow, GroundLeaseTemplate)> LoadAsync(PurchaseClosed @event, IQuerySession session, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(session);

        var workflow = await session.LoadAggregateAsync<PurchaseWorkflow>(@event.AggregateId, ct).ConfigureAwait(false);
        var template = await session.LoadAggregateAsync<GroundLeaseTemplate>(workflow.GroundLeaseTemplateId, ct).ConfigureAwait(false);

        return (workflow, template);
    }

    /// <summary>Starts the ground lease that makes the workflow's buyer the homeowner.</summary>
    public static IStartStream Handle(PurchaseClosed @event, PurchaseWorkflow workflow, GroundLeaseTemplate template)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var leaseId = GroundLeaseId.Generate();
        var created = GroundLease.CreateFromPurchase(leaseId, template, workflow, @event.ActualClosingDate, UserId.Create(@event.RecordedBy));

        return MartenOps.StartStream<GroundLease>(leaseId.Value, created);
    }
}
