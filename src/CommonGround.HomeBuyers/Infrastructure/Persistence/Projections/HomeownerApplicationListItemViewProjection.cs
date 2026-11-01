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
}