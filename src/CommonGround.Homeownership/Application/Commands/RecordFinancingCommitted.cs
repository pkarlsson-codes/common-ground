using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

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
    public static FinancingCommitted? Handle(RecordFinancingCommitted command, [WriteAggregate(nameof(RecordFinancingCommitted.WorkflowId))] PurchaseWorkflow workflow)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(workflow);

        return workflow.RecordFinancingCommitted(
            command.LenderName,
            command.CommittedAmount,
            command.PreapprovalLetterDate,
            command.RecordedBy);
    }
}
