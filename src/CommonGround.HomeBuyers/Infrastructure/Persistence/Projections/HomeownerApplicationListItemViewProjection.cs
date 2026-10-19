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

    public static void Apply(HomeownerApplicationListItemView view, ApplicationApproved _)
    {
        view.Phase = ApplicationPhase.Approved.DisplayName;
    }

    public static void Apply(HomeownerApplicationListItemView view, ApplicationSelectedForHome _)
    {
        view.Phase = ApplicationPhase.Selected.DisplayName;
    }

    public static void Apply(HomeownerApplicationListItemView view, ApplicationDenied _)
    {
        view.Phase = ApplicationPhase.Denied.DisplayName;
    }

    public static void Apply(HomeownerApplicationListItemView view, ApplicationWithdrawn _)
    {
        view.Phase = ApplicationPhase.Withdrawn.DisplayName;
    }
}