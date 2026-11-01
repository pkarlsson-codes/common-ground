using CommonGround.Homeownership.Application.ReadModels;
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
        };

    public static void Apply(PurchaseWorkflowListItemView view, BuyerEligibilityVerified e) => MoveOn(view, "InProgress", e.OccurredAt);
    public static void Apply(PurchaseWorkflowListItemView view, FinancingCommitted e) => MoveOn(view, "InProgress", e.OccurredAt);
    public static void Apply(PurchaseWorkflowListItemView view, InspectionCompleted e) => MoveOn(view, "InProgress", e.OccurredAt);
    public static void Apply(PurchaseWorkflowListItemView view, TitleCleared e) => MoveOn(view, "InProgress", e.OccurredAt);
    public static void Apply(PurchaseWorkflowListItemView view, LeaseExecuted e) => MoveOn(view, "InProgress", e.OccurredAt);

    public static void Apply(PurchaseWorkflowListItemView view, ClosingScheduled e)
    {
        MoveOn(view, "ClosingScheduled", e.OccurredAt);
        view.ScheduledClosingDate = e.ScheduledClosingDate;
    }

    public static void Apply(PurchaseWorkflowListItemView view, PurchaseClosed e) => MoveOn(view, "Closed", e.OccurredAt);
    public static void Apply(PurchaseWorkflowListItemView view, PurchaseFailed e) => MoveOn(view, "Failed", e.OccurredAt);
    public static void Apply(PurchaseWorkflowListItemView view, PurchaseWithdrawn e) => MoveOn(view, "Withdrawn", e.OccurredAt);
    public static void Apply(PurchaseWorkflowListItemView view, WorkflowStalled e) => MoveOn(view, "Stalled", e.OccurredAt);

    private static void MoveOn(PurchaseWorkflowListItemView view, string phase, DateTimeOffset at)
    {
        view.Phase = phase;
        view.StepEnteredAt = at;
    }
}
