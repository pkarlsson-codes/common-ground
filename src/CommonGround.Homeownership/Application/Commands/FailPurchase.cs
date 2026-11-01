using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Records that a purchase has failed.</summary>
public sealed record FailPurchase(
    PurchaseWorkflowId WorkflowId,
    string FailureReason,
    DateOnly FailedDate,
    UserId RecordedBy);

/// <summary>Handles <see cref="FailPurchase"/>.</summary>
public static class FailPurchaseHandler
{
    /// <summary>Loads the workflow, records the failure, and persists.</summary>
    public static PurchaseFailed Handle(FailPurchase command, [WriteAggregate(nameof(FailPurchase.WorkflowId))] PurchaseWorkflow workflow)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(workflow);

        return workflow.FailPurchase(command.FailureReason, command.FailedDate, command.RecordedBy);
    }
}
