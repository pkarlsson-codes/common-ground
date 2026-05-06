using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Records that financing has been committed by a lender.</summary>
public sealed record RecordFinancingCommitted(
    PurchaseWorkflowId WorkflowId,
    string LenderName,
    decimal CommittedAmount,
    DateOnly PreapprovalLetterDate,
    UserId RecordedBy);

/// <summary>Handles <see cref="RecordFinancingCommitted"/>.</summary>
public static class RecordFinancingCommittedHandler
{
    /// <summary>Loads the workflow, records financing commitment, and persists.</summary>
    public static async Task Handle(
        RecordFinancingCommitted command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var stream = await session.Events.FetchForWriting<PurchaseWorkflow>(command.WorkflowId.Value, cancellationToken).ConfigureAwait(false);
        var workflow = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(PurchaseWorkflow), command.WorkflowId.Value);

        workflow.RecordFinancingCommitted(command.LenderName, command.CommittedAmount, command.PreapprovalLetterDate, command.RecordedBy);

        workflow.ValidateInvariants();
        stream.AppendMany(workflow.UncommittedEvents.ToArray());
        workflow.UncommittedEvents.MarkAsCommitted();
    }
}
