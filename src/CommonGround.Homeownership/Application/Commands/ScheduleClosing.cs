using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Schedules the closing date for a purchase workflow.</summary>
public sealed record ScheduleClosing(
    PurchaseWorkflowId WorkflowId,
    DateOnly ScheduledClosingDate,
    UserId RecordedBy);

/// <summary>Handles <see cref="ScheduleClosing"/>.</summary>
public static class ScheduleClosingHandler
{
    /// <summary>Loads the workflow, schedules the closing date, and persists.</summary>
    public static ClosingScheduled Handle(ScheduleClosing command, [WriteAggregate(nameof(ScheduleClosing.WorkflowId))] PurchaseWorkflow workflow)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(workflow);

        return workflow.ScheduleClosing(command.ScheduledClosingDate, command.RecordedBy);
    }
}
