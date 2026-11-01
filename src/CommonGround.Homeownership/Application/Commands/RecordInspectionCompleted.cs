using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Records that a home inspection has been completed.</summary>
public sealed record RecordInspectionCompleted(
    PurchaseWorkflowId WorkflowId,
    DateOnly InspectionDate,
    string InspectorName,
    string? Notes,
    UserId RecordedBy);

/// <summary>Handles <see cref="RecordInspectionCompleted"/>.</summary>
public static class RecordInspectionCompletedHandler
{
    /// <summary>Loads the workflow, records inspection completion, and persists.</summary>
    public static InspectionCompleted? Handle(RecordInspectionCompleted command, [WriteAggregate(nameof(RecordInspectionCompleted.WorkflowId))] PurchaseWorkflow workflow)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(workflow);

        return workflow.RecordInspectionCompleted(
            command.InspectionDate,
            command.InspectorName,
            command.Notes,
            command.RecordedBy);
    }
}
