using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Records that buyer eligibility has been verified, with the income and household size the documents show.</summary>
public sealed record RecordEligibilityVerified(
    PurchaseWorkflowId WorkflowId,
    DateOnly VerifiedDate,
    Money VerifiedAnnualIncome,
    int VerifiedHouseholdSize,
    string? Notes,
    UserId RecordedBy);

/// <summary>Handles <see cref="RecordEligibilityVerified"/>.</summary>
public static class RecordEligibilityVerifiedHandler
{
    /// <summary>Loads the workflow, records eligibility verification, and persists.</summary>
    public static BuyerEligibilityVerified? Handle(RecordEligibilityVerified command, [WriteAggregate(nameof(RecordEligibilityVerified.WorkflowId))] PurchaseWorkflow workflow)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(workflow);

        return workflow.RecordEligibilityVerified(
            command.VerifiedDate, command.VerifiedAnnualIncome, command.VerifiedHouseholdSize, command.Notes, command.RecordedBy);
    }
}
