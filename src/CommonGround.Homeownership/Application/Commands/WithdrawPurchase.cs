using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Records that a buyer has withdrawn from a purchase.</summary>
public sealed record WithdrawPurchase(
    PurchaseWorkflowId WorkflowId,
    DateOnly WithdrawnDate,
    string? Reason,
    UserId RecordedBy);

/// <summary>Handles <see cref="WithdrawPurchase"/>.</summary>
public static class WithdrawPurchaseHandler
{
    /// <summary>Loads the workflow, records buyer withdrawal, and persists.</summary>
    public static PurchaseWithdrawn Handle(WithdrawPurchase command, [WriteAggregate(nameof(WithdrawPurchase.WorkflowId))] PurchaseWorkflow workflow)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(workflow);

        return workflow.WithdrawPurchase(command.WithdrawnDate, command.Reason, command.RecordedBy);
    }
}
