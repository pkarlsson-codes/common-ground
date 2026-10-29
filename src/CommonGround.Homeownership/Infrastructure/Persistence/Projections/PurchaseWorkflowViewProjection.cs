using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;
using JasperFx.Events;
using Marten.Events.Aggregation;

namespace CommonGround.Homeownership.Infrastructure.Persistence.Projections;

/// <summary>Builds <see cref="PurchaseWorkflowView"/> from PurchaseWorkflow events.</summary>
internal sealed class PurchaseWorkflowViewProjection : SingleStreamProjection<PurchaseWorkflowView, string>
{
    public static PurchaseWorkflowView Create(IEvent<PurchaseWorkflowInitiated> e) =>
        new()
        {
            Id = e.Data.AggregateId,
            GroundLeaseTemplateId = e.Data.GroundLeaseTemplateId,
            LandParcelId = e.Data.LandParcelId,
            HomeId = e.Data.HomeId,
            ApplicationId = e.Data.ApplicationId,
            HouseholdId = e.Data.HouseholdId,
            Phase = "Initiated",
            InitiatedBy = e.Data.InitiatedBy,
            BuyerFullName = e.Data.BuyerFullName,
            BuyerEmail = e.Data.BuyerEmail,
            BuyerPhone = e.Data.BuyerPhone,
            PurchasePrice = e.Data.PurchasePrice,
            CreatedAt = e.Data.OccurredAt,
        };

    public static void Apply(PurchaseWorkflowView view, BuyerEligibilityVerified e)
    {
        view.Phase = "InProgress";
        view.EligibilityDetails = new EligibilityDetailsEntry
        {
            VerifiedDate = e.VerifiedDate,
            Notes = e.EligibilityNotes,
        };
    }

    public static void Apply(PurchaseWorkflowView view, FinancingCommitted e)
    {
        view.Phase = "InProgress";
        view.FinancingDetails = new FinancingDetailsEntry
        {
            LenderName = e.LenderName,
            CommittedAmount = e.CommittedAmount,
            PreapprovalLetterDate = e.PreapprovalLetterDate,
        };
    }

    public static void Apply(PurchaseWorkflowView view, InspectionCompleted e)
    {
        view.Phase = "InProgress";
        view.InspectionResult = new InspectionResultEntry
        {
            InspectionDate = e.InspectionDate,
            InspectorName = e.InspectorName,
            Notes = e.InspectionNotes,
        };
    }

    public static void Apply(PurchaseWorkflowView view, TitleCleared e)
    {
        view.Phase = "InProgress";
        view.TitleDetails = new TitleDetailsEntry
        {
            ClearedDate = e.ClearedDate,
            TitleCompanyName = e.TitleCompanyName,
        };
    }

    public static void Apply(PurchaseWorkflowView view, LeaseExecuted e)
    {
        view.Phase = "InProgress";
        view.LeaseExecutionDetails = new LeaseExecutionDetailsEntry
        {
            SignedDate = e.SignedDate,
        };
    }

    public static void Apply(PurchaseWorkflowView view, ClosingScheduled e)
    {
        view.Phase = "ClosingScheduled";
        view.ScheduledClosingDate = e.ScheduledClosingDate;
    }

    public static void Apply(PurchaseWorkflowView view, PurchaseClosed e)
    {
        view.Phase = "Closed";
        view.ActualClosingDate = e.ActualClosingDate;
        view.ClosingNotes = e.ClosingNotes;
    }

    public static void Apply(PurchaseWorkflowView view, PurchaseFailed e)
    {
        view.Phase = "Failed";
        view.FailureReason = e.FailureReason;
    }

    public static void Apply(PurchaseWorkflowView view, PurchaseWithdrawn e)
    {
        view.Phase = "Withdrawn";
        view.WithdrawalReason = e.WithdrawalReason;
    }

    public static void Apply(PurchaseWorkflowView view, WorkflowStalled e)
    {
        view.Phase = "Stalled";
        view.StalledReason = e.StalledReason;
        view.StalledDate = e.StalledDate;
        view.ResolutionTargetDate = e.ResolutionTargetDate;
    }
}
