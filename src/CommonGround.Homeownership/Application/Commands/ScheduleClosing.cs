using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using Marten;

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
    public static async Task Handle(
        ScheduleClosing command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var workflow = await session
            .LoadAggregateAsync<PurchaseWorkflow>(
                command.WorkflowId,
                cancellationToken)
            .ConfigureAwait(false);

        workflow.ScheduleClosing(command.ScheduledClosingDate, command.RecordedBy);

        session.UpdateAggregate(workflow);
    }
}
