using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Records that the home's appraisal has been completed.</summary>
public sealed record RecordAppraisalCompleted(
    PurchaseWorkflowId WorkflowId,
    decimal AppraisedValue,
    DateOnly AppraisalDate,
    string AppraiserName,
    string? Notes,
    UserId RecordedBy);

/// <summary>Handles <see cref="RecordAppraisalCompleted"/>.</summary>
public static class RecordAppraisalCompletedHandler
{
    /// <summary>Loads the workflow, records the appraisal, and persists.</summary>
    public static AppraisalCompleted? Handle(RecordAppraisalCompleted command, [WriteAggregate(nameof(RecordAppraisalCompleted.WorkflowId))] PurchaseWorkflow workflow)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(workflow);

        return workflow.RecordAppraisalCompleted(
            command.AppraisedValue,
            command.AppraisalDate,
            command.AppraiserName,
            command.Notes,
            command.RecordedBy);
    }
}
