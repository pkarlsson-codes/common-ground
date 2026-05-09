using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
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

        var workflow = await session
            .LoadAggregateAsync<PurchaseWorkflow>(
                command.WorkflowId,
                cancellationToken)
            .ConfigureAwait(false);

        workflow.RecordFinancingCommitted(
            command.LenderName,
            command.CommittedAmount,
            command.PreapprovalLetterDate,
            command.RecordedBy);
        
        session.UpdateAggregate(workflow);
    }
}
