using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

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
    public static async Task Handle(
        RecordLeaseExecuted command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var stream = await session.Events.FetchForWriting<PurchaseWorkflow>(command.WorkflowId.Value, cancellationToken).ConfigureAwait(false);
        var workflow = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(PurchaseWorkflow), command.WorkflowId.Value);

        workflow.RecordLeaseExecuted(command.SignedDate, command.RecordedBy);

        workflow.ValidateInvariants();
        stream.AppendMany(workflow.UncommittedEvents.ToArray());
        workflow.UncommittedEvents.MarkAsCommitted();
    }
}
