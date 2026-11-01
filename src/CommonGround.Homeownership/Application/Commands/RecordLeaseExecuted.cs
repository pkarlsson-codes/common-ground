using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Records that the ground lease has been signed by both parties.</summary>
public sealed record RecordLeaseExecuted(
    PurchaseWorkflowId WorkflowId,
    DateOnly SignedDate,
    UserId RecordedBy);

/// <summary>Handles <see cref="RecordLeaseExecuted"/>.</summary>
public static class RecordLeaseExecutedHandler
{
    /// <summary>Loads the workflow, records lease execution, and persists.</summary>
    public static LeaseExecuted? Handle(RecordLeaseExecuted command, [WriteAggregate(nameof(RecordLeaseExecuted.WorkflowId))] PurchaseWorkflow workflow)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(workflow);

        return workflow.RecordLeaseExecuted(command.SignedDate, command.RecordedBy);
    }
}
