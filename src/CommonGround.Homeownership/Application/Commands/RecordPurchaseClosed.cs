using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Records that a purchase has been closed.</summary>
public sealed record RecordPurchaseClosed(
    PurchaseWorkflowId WorkflowId,
    DateOnly ActualClosingDate,
    string? Notes,
    UserId RecordedBy);

/// <summary>Handles <see cref="RecordPurchaseClosed"/>.</summary>
public static class RecordPurchaseClosedHandler
{
    /// <summary>Handles the command.</summary>
    public static PurchaseClosed Handle(RecordPurchaseClosed command, [WriteAggregate(nameof(RecordPurchaseClosed.WorkflowId))] PurchaseWorkflow workflow)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(workflow);

        return workflow.RecordPurchaseClosed(command.ActualClosingDate, command.Notes, command.RecordedBy);
    }
}
