using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Corrects the recorded financing commitment of a purchase.</summary>
public sealed record CorrectFinancing(
    PurchaseWorkflowId WorkflowId,
    string LenderName,
    decimal CommittedAmount,
    DateOnly PreapprovalLetterDate,
    string? Reason,
    UserId CorrectedBy);

/// <summary>Handles <see cref="CorrectFinancing"/>.</summary>
public static class CorrectFinancingHandler
{
    /// <summary>Loads the workflow, corrects financing, and persists.</summary>
    public static FinancingCorrected Handle(CorrectFinancing command, [WriteAggregate(nameof(CorrectFinancing.WorkflowId))] PurchaseWorkflow workflow)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(workflow);

        return workflow.CorrectFinancing(
            command.LenderName, command.CommittedAmount, command.PreapprovalLetterDate,
            command.Reason, command.CorrectedBy);
    }
}
