using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

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
    public static async Task Handle(
        RecordInspectionCompleted command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var stream = await session.Events.FetchForWriting<PurchaseWorkflow>(command.WorkflowId.Value, cancellationToken).ConfigureAwait(false);
        var workflow = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(PurchaseWorkflow), command.WorkflowId.Value);

        workflow.RecordInspectionCompleted(command.InspectionDate, command.InspectorName, command.Notes, command.RecordedBy);

        workflow.ValidateInvariants();
        stream.AppendMany(workflow.UncommittedEvents.ToArray());
        workflow.UncommittedEvents.MarkAsCommitted();
    }
}
