using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Records that a purchase workflow is stalled.</summary>
public sealed record StallWorkflow(
    PurchaseWorkflowId WorkflowId,
    string StalledReason,
    DateOnly StalledDate,
    DateOnly ResolutionTargetDate,
    UserId RecordedBy);

/// <summary>Handles <see cref="StallWorkflow"/>.</summary>
public static class StallWorkflowHandler
{
    /// <summary>Loads the workflow, marks it stalled, and persists.</summary>
    public static WorkflowStalled Handle(StallWorkflow command, [WriteAggregate(nameof(StallWorkflow.WorkflowId))] PurchaseWorkflow workflow)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(workflow);

        return workflow.StallWorkflow(command.StalledReason, command.StalledDate, command.ResolutionTargetDate, command.RecordedBy);
    }
}
