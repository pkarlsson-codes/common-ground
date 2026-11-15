using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Corrects the recorded appraisal of a purchase.</summary>
public sealed record CorrectAppraisal(
    PurchaseWorkflowId WorkflowId,
    decimal AppraisedValue,
    DateOnly AppraisalDate,
    string AppraiserName,
    string? Notes,
    string? Reason,
    UserId CorrectedBy);

/// <summary>Handles <see cref="CorrectAppraisal"/>.</summary>
public static class CorrectAppraisalHandler
{
    /// <summary>Loads the workflow, corrects the appraisal, and persists.</summary>
    public static AppraisalCorrected Handle(CorrectAppraisal command, [WriteAggregate(nameof(CorrectAppraisal.WorkflowId))] PurchaseWorkflow workflow)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(workflow);

        return workflow.CorrectAppraisal(
            command.AppraisedValue, command.AppraisalDate, command.AppraiserName,
            command.Notes, command.Reason, command.CorrectedBy);
    }
}
