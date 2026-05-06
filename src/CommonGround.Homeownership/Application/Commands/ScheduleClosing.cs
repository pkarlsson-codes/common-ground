using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
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

        var stream = await session.Events.FetchForWriting<PurchaseWorkflow>(command.WorkflowId.Value, cancellationToken).ConfigureAwait(false);
        var workflow = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(PurchaseWorkflow), command.WorkflowId.Value);

        workflow.ScheduleClosing(command.ScheduledClosingDate, command.RecordedBy);

        workflow.ValidateInvariants();
        stream.AppendMany(workflow.UncommittedEvents.ToArray());
        workflow.UncommittedEvents.MarkAsCommitted();
    }
}
