using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;
using JasperFx.Events;
using Marten.Events.Aggregation;

namespace CommonGround.Homeownership.Infrastructure.Persistence.Projections;

/// <summary>Builds <see cref="PurchaseWorkflowListItemView"/> from PurchaseWorkflow events.</summary>
internal sealed class PurchaseWorkflowListItemViewProjection : SingleStreamProjection<PurchaseWorkflowListItemView, string>
{
    public static PurchaseWorkflowListItemView Create(IEvent<PurchaseWorkflowInitiated> e) =>
        new()
        {
            Id = e.Data.AggregateId,
            GroundLeaseTemplateId = e.Data.GroundLeaseTemplateId,
            HomeId = e.Data.HomeId,
            HouseholdId = e.Data.HouseholdId,
            BuyerFullName = e.Data.BuyerFullName,
            PurchasePrice = e.Data.PurchasePrice,
            Phase = "Initiated",
            CreatedAt = e.Data.OccurredAt,
            StepEnteredAt = e.Data.OccurredAt,
            NextStep = PurchaseStep.Eligibility.DisplayName,
        };

    public static void Apply(PurchaseWorkflowListItemView view, BuyerEligibilityVerified e) => Record(view, PurchaseStep.Eligibility, e.OccurredAt);
    public static void Apply(PurchaseWorkflowListItemView view, AppraisalCompleted e) => Record(view, PurchaseStep.Appraisal, e.OccurredAt);
    public static void Apply(PurchaseWorkflowListItemView view, FinancingCommitted e) => Record(view, PurchaseStep.Financing, e.OccurredAt);
    public static void Apply(PurchaseWorkflowListItemView view, InspectionCompleted e) => Record(view, PurchaseStep.Inspection, e.OccurredAt);
    public static void Apply(PurchaseWorkflowListItemView view, TitleCleared e) => Record(view, PurchaseStep.Title, e.OccurredAt);
    public static void Apply(PurchaseWorkflowListItemView view, LeaseExecuted e) => Record(view, PurchaseStep.LeaseSigning, e.OccurredAt);

    public static void Apply(PurchaseWorkflowListItemView view, ClosingScheduled e)
    {
        MoveOn(view, "ClosingScheduled", e.OccurredAt);
        view.ScheduledClosingDate = e.ScheduledClosingDate;
    }

    public static void Apply(PurchaseWorkflowListItemView view, PurchaseClosed e) => MoveOn(view, "Closed", e.OccurredAt);
    public static void Apply(PurchaseWorkflowListItemView view, PurchaseFailed e) => MoveOn(view, "Failed", e.OccurredAt);
    public static void Apply(PurchaseWorkflowListItemView view, PurchaseWithdrawn e) => MoveOn(view, "Withdrawn", e.OccurredAt);
    public static void Apply(PurchaseWorkflowListItemView view, WorkflowStalled e) => MoveOn(view, "Stalled", e.OccurredAt);

    private static void Record(PurchaseWorkflowListItemView view, PurchaseStep step, DateTimeOffset at)
    {
        MoveOn(view, "InProgress", at);
        if (!view.RecordedSteps.Contains(step.DisplayName))
            view.RecordedSteps.Add(step.DisplayName);

        bool Recorded(PurchaseStep s) => view.RecordedSteps.Contains(s.DisplayName);
        view.NextStep = PurchaseStep.WaitingOn(
            Recorded(PurchaseStep.Eligibility),
            Recorded(PurchaseStep.Appraisal),
            Recorded(PurchaseStep.Financing),
            Recorded(PurchaseStep.Inspection),
            Recorded(PurchaseStep.Title),
            Recorded(PurchaseStep.LeaseSigning)).DisplayName;
    }

    private static void MoveOn(PurchaseWorkflowListItemView view, string phase, DateTimeOffset at)
    {
        view.Phase = phase;
        view.StepEnteredAt = at;
    }
}
