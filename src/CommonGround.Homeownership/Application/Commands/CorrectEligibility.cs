using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Corrects the recorded eligibility verification of a purchase. Null or empty income documents keep the recorded ones.</summary>
public sealed record CorrectEligibility(
    PurchaseWorkflowId WorkflowId,
    DateOnly VerifiedDate,
    Money VerifiedAnnualIncome,
    int VerifiedHouseholdSize,
    IReadOnlyList<string>? IncomeDocumentIds,
    string? Notes,
    string? Reason,
    UserId CorrectedBy);

/// <summary>Handles <see cref="CorrectEligibility"/>.</summary>
public static class CorrectEligibilityHandler
{
    /// <summary>Loads the workflow, corrects eligibility, and persists.</summary>
    public static BuyerEligibilityCorrected Handle(CorrectEligibility command, [WriteAggregate(nameof(CorrectEligibility.WorkflowId))] PurchaseWorkflow workflow)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(workflow);

        return workflow.CorrectEligibility(
            command.VerifiedDate, command.VerifiedAnnualIncome, command.VerifiedHouseholdSize,
            command.IncomeDocumentIds, command.Notes, command.Reason, command.CorrectedBy);
    }
}
