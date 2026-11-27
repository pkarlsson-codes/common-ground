using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeownerApplications.Events;
using JasperFx.Events;
using Marten.Events.Aggregation;

namespace CommonGround.HomeBuyers.Infrastructure.Persistence.Projections;

/// <summary>Builds <see cref="HomeownerApplicationView"/> from HomeownerApplication events.</summary>
internal sealed class HomeownerApplicationViewProjection : SingleStreamProjection<HomeownerApplicationView, string>
{
    public static HomeownerApplicationView Create(IEvent<ApplicationSubmitted> e) =>
        new()
        {
            Id = e.Data.AggregateId,
            HouseholdId = e.Data.HouseholdId,
            HouseholdSize = e.Data.HouseholdSize,
            AnnualIncome = e.Data.AnnualIncome.Amount,
            AccessibilityRequired = e.Data.AccessibilityRequired,
            Phase = ApplicationPhase.Submitted.DisplayName,
            SubmittedAt = e.Data.OccurredAt,
            SubmittedBy = e.Data.SubmittedBy,
        };

    public static void Apply(HomeownerApplicationView view, ApplicationApproved e)
    {
        view.Phase = ApplicationPhase.Approved.DisplayName;
        view.ApprovedDate = e.ApprovedDate;
    }

    public static void Apply(HomeownerApplicationView view, ApplicationDenied e)
    {
        view.Phase = ApplicationPhase.Denied.DisplayName;
        view.DenialReason = e.Reason;
    }

    public static void Apply(HomeownerApplicationView view, ApplicationSelectedForHome e)
    {
        view.Phase = ApplicationPhase.Selected.DisplayName;
        view.SelectedDate = e.SelectedDate;
        view.SelectedHomeId = e.HomeId;
        view.SelectedLandParcelId = e.LandParcelId;
        view.SelectedBy = e.SelectedBy;
    }

    public static void Apply(HomeownerApplicationView view, ApplicationReturnedToWaitingList _)
    {
        view.Phase = ApplicationPhase.Approved.DisplayName;
        view.SelectedDate = null;
        view.SelectedHomeId = null;
        view.SelectedLandParcelId = null;
        view.SelectedBy = null;
    }

    public static void Apply(HomeownerApplicationView view, ApplicationPurchaseClosed _)
    {
        view.Phase = ApplicationPhase.Purchased.DisplayName;
    }

    public static void Apply(HomeownerApplicationView view, ApplicationWithdrawn e)
    {
        view.Phase = ApplicationPhase.Withdrawn.DisplayName;
        view.WithdrawalReason = e.Reason;
    }

    public static void Apply(HomeownerApplicationView view, ApplicationRemovedFromWaitingList e)
    {
        view.Phase = ApplicationPhase.Removed.DisplayName;
        view.RemovalReason = e.Reason.DisplayName;
        view.RemovalNote = e.Note;
        view.RemovedDate = e.RemovedDate;
    }

    public static void Apply(HomeownerApplicationView view, ApplicationDeclarationUpdated e)
    {
        view.HouseholdSize = e.HouseholdSize;
        view.AnnualIncome = e.AnnualIncome.Amount;
        view.AccessibilityRequired = e.AccessibilityRequired;
        view.QualificationToConfirmSince = e.QualificationToConfirm?.Since;
        view.QualifiedAnnualIncome = e.QualificationToConfirm?.QualifiedAnnualIncome.Amount;
        view.QualifiedHouseholdSize = e.QualificationToConfirm?.QualifiedHouseholdSize;
    }

    public static void Apply(HomeownerApplicationView view, ApplicationQualificationConfirmed e)
    {
        view.QualificationToConfirmSince = null;
        view.QualifiedAnnualIncome = null;
        view.QualifiedHouseholdSize = null;
        view.QualificationConfirmedDate = e.ConfirmedDate;
        view.QualificationConfirmedBy = e.ConfirmedBy;
    }
}
