using CommonGround.ApplicantSelection.Application.ReadModels;
using CommonGround.ApplicantSelection.Domain.HomeownerApplications;
using CommonGround.ApplicantSelection.Domain.HomeownerApplications.Events;
using JasperFx.Events;
using Marten.Events.Aggregation;

namespace CommonGround.ApplicantSelection.Infrastructure.Persistence.Projections;

/// <summary>Builds <see cref="HomeownerApplicationListItemView"/> from HomeownerApplication events.</summary>
internal sealed class HomeownerApplicationListItemViewProjection : SingleStreamProjection<HomeownerApplicationListItemView, string>
{
    public static HomeownerApplicationListItemView Create(IEvent<ApplicationSubmitted> e) =>
        new()
        {
            Id = e.Data.AggregateId,
            ApplicantFullName = e.Data.ApplicantFullName,
            ApplicantEmail = e.Data.ApplicantEmail,
            ApplicantPhone = e.Data.ApplicantPhone,
            HouseholdSize = e.Data.HouseholdSize,
            AccessibilityRequired = e.Data.AccessibilityRequired,
            Phase = ApplicationPhase.Submitted.DisplayName,
            SubmittedAt = e.Data.OccurredAt,
        };

    public static void Apply(HomeownerApplicationListItemView view, EligibilityApproved _)
    {
        view.Phase = ApplicationPhase.Active.DisplayName;
    }

    public static void Apply(HomeownerApplicationListItemView view, EligibilityDenied _)
    {
        view.Phase = ApplicationPhase.EligibilityDenied.DisplayName;
    }

    public static void Apply(HomeownerApplicationListItemView view, MortgagePrequalificationExpired _)
    {
        view.Phase = ApplicationPhase.Active.DisplayName;
    }

    public static void Apply(HomeownerApplicationListItemView view, AddedToWaitingList _)
    {
        view.Phase = ApplicationPhase.Waitlisted.DisplayName;
    }

    public static void Apply(HomeownerApplicationListItemView view, ApplicantSelectedForHome _)
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