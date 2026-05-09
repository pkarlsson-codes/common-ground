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
            BuyerFullName = e.Data.BuyerFullName,
            PurchasePrice = e.Data.PurchasePrice,
            Phase = "Initiated",
            CreatedAt = e.Data.OccurredAt,
        };

    public static void Apply(PurchaseWorkflowListItemView view, BuyerEligibilityVerified _) => view.Phase = "InProgress";
    public static void Apply(PurchaseWorkflowListItemView view, FinancingCommitted _) => view.Phase = "InProgress";
    public static void Apply(PurchaseWorkflowListItemView view, InspectionCompleted _) => view.Phase = "InProgress";
    public static void Apply(PurchaseWorkflowListItemView view, TitleCleared _) => view.Phase = "InProgress";
    public static void Apply(PurchaseWorkflowListItemView view, LeaseExecuted _) => view.Phase = "InProgress";
    public static void Apply(PurchaseWorkflowListItemView view, ClosingScheduled _) => view.Phase = "ClosingScheduled";
    public static void Apply(PurchaseWorkflowListItemView view, PurchaseClosed _) => view.Phase = "Closed";
    public static void Apply(PurchaseWorkflowListItemView view, PurchaseFailed _) => view.Phase = "Failed";
    public static void Apply(PurchaseWorkflowListItemView view, PurchaseWithdrawn _) => view.Phase = "Withdrawn";
    public static void Apply(PurchaseWorkflowListItemView view, WorkflowStalled _) => view.Phase = "Stalled";
}
