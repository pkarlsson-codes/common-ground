using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeownerApplications.Events;
using JasperFx.Events;
using Marten.Events.Aggregation;

namespace CommonGround.HomeBuyers.Infrastructure.Persistence.Projections;

/// <summary>Builds <see cref="HomeownerApplicationListItemView"/> from HomeownerApplication events.</summary>
internal sealed class HomeownerApplicationListItemViewProjection : SingleStreamProjection<HomeownerApplicationListItemView, string>
{
    public static HomeownerApplicationListItemView Create(IEvent<ApplicationSubmitted> e) =>
        new()
        {
            Id = e.Data.AggregateId,
            HouseholdId = e.Data.HouseholdId,
            HouseholdSize = e.Data.HouseholdSize,
            AnnualIncome = e.Data.AnnualIncome.Amount,
            AccessibilityRequired = e.Data.AccessibilityRequired,
            Phase = ApplicationPhase.Submitted.DisplayName,
            SubmittedAt = e.Data.OccurredAt,
        };

    public static void Apply(HomeownerApplicationListItemView view, ApplicationApproved e)
    {
        view.Phase = ApplicationPhase.Approved.DisplayName;
        view.ApprovedDate = e.ApprovedDate;
    }

    public static void Apply(HomeownerApplicationListItemView view, ApplicationSelectedForHome e)
    {
        view.Phase = ApplicationPhase.Selected.DisplayName;
        view.SelectedHomeId = e.HomeId;
        view.SelectedDate = e.SelectedDate;
    }

    public static void Apply(HomeownerApplicationListItemView view, ApplicationReturnedToWaitingList _)
    {
        view.Phase = ApplicationPhase.Approved.DisplayName;
        view.SelectedHomeId = null;
        view.SelectedDate = null;
    }

    public static void Apply(HomeownerApplicationListItemView view, ApplicationPurchaseClosed e)
    {
        view.Phase = ApplicationPhase.Purchased.DisplayName;
        view.PurchasedDate = e.ClosingDate;
    }

    public static void Apply(HomeownerApplicationListItemView view, ApplicationDenied e)
    {
        view.Phase = ApplicationPhase.Denied.DisplayName;
        view.ClosedDate = e.DeniedDate;
    }

    public static void Apply(HomeownerApplicationListItemView view, ApplicationWithdrawn e)
    {
        view.Phase = ApplicationPhase.Withdrawn.DisplayName;
        view.ClosedDate = e.WithdrawnDate;
    }

    public static void Apply(HomeownerApplicationListItemView view, ApplicationRemovedFromWaitingList e)
    {
        view.Phase = ApplicationPhase.Removed.DisplayName;
        view.ClosedDate = e.RemovedDate;
    }

    public static void Apply(HomeownerApplicationListItemView view, ApplicationDeclarationUpdated e)
    {
        view.HouseholdSize = e.HouseholdSize;
        view.AnnualIncome = e.AnnualIncome.Amount;
        view.AccessibilityRequired = e.AccessibilityRequired;
        view.QualificationToConfirmSince = e.QualificationToConfirm?.Since;
        view.QualifiedAnnualIncome = e.QualificationToConfirm?.QualifiedAnnualIncome.Amount;
        view.QualifiedHouseholdSize = e.QualificationToConfirm?.QualifiedHouseholdSize;
    }

    public static void Apply(HomeownerApplicationListItemView view, ApplicationQualificationConfirmed _)
    {
        view.QualificationToConfirmSince = null;
        view.QualifiedAnnualIncome = null;
        view.QualifiedHouseholdSize = null;
    }
}
