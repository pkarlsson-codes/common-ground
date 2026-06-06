using CommonGround.ApplicantSelection.Application.ReadModels;
using CommonGround.ApplicantSelection.Domain.HomeownerApplications;
using CommonGround.ApplicantSelection.Domain.HomeownerApplications.Events;
using JasperFx.Events;
using Marten.Events.Aggregation;

namespace CommonGround.ApplicantSelection.Infrastructure.Persistence.Projections;

/// <summary>Builds <see cref="HomeownerApplicationView"/> from HomeownerApplication events.</summary>
internal sealed class HomeownerApplicationViewProjection : SingleStreamProjection<HomeownerApplicationView, string>
{
    public static HomeownerApplicationView Create(IEvent<ApplicationSubmitted> e) =>
        new()
        {
            Id = e.Data.AggregateId,
            ApplicantFullName = e.Data.ApplicantFullName,
            ApplicantEmail = e.Data.ApplicantEmail,
            ApplicantPhone = e.Data.ApplicantPhone,
            HouseholdSize = e.Data.HouseholdSize,
            AnnualIncome = e.Data.AnnualIncome,
            AccessibilityRequired = e.Data.AccessibilityRequired,
            Phase = ApplicationPhase.Submitted.DisplayName,
            SubmittedAt = e.Data.OccurredAt,
            SubmittedBy = e.Data.SubmittedBy,
        };

    public static void Apply(HomeownerApplicationView view, EligibilityApproved e)
    {
        view.Phase = ApplicationPhase.Active.DisplayName;
        view.EligibilityApprovedDate = e.ApprovedDate;
    }

    public static void Apply(HomeownerApplicationView view, EligibilityDenied e)
    {
        view.Phase = ApplicationPhase.EligibilityDenied.DisplayName;
        view.DenialReason = e.Reason;
    }

    public static void Apply(HomeownerApplicationView view, HousingCounselingRecorded e)
    {
        view.IsHousingCounselingCompleted = true;
        view.HousingCounselingCertificationNumber = e.CertificationNumber;
        view.HousingCounselingCompletedDate = e.CompletedDate;
        view.HousingCounselorOrganization = e.CounselorOrganization;
    }

    public static void Apply(HomeownerApplicationView view, CltOrientationRecorded e)
    {
        view.IsCltOrientationCompleted = true;
        view.CltOrientationCompletedDate = e.CompletedDate;
    }

    public static void Apply(HomeownerApplicationView view, MortgagePrequalificationRecorded e)
    {
        view.MortgagePrequalification = new MortgagePrequalificationEntry
        {
            LenderName = e.LenderName,
            ApprovedAmount = e.ApprovedAmount,
            ExpiryDate = e.ExpiryDate,
        };
    }

    public static void Apply(HomeownerApplicationView view, MortgagePrequalificationExpired _)
    {
        view.MortgagePrequalification = null;
    }

    public static void Apply(HomeownerApplicationView view, AddedToWaitingList e)
    {
        view.Phase = ApplicationPhase.Waitlisted.DisplayName;
        view.AddedToWaitingListDate = e.AddedDate;
    }

    public static void Apply(HomeownerApplicationView view, ApplicantSelectedForHome e)
    {
        view.Phase = ApplicationPhase.Selected.DisplayName;
        view.SelectedDate = e.SelectedDate;
        view.SelectedHomeId = e.HomeId;
        view.SelectedLandParcelId = e.LandParcelId;
        view.SelectedBy = e.SelectedBy;
    }

    public static void Apply(HomeownerApplicationView view, ApplicationDenied e)
    {
        view.Phase = ApplicationPhase.Denied.DisplayName;
        view.DenialReason = e.Reason;
    }

    public static void Apply(HomeownerApplicationView view, ApplicationWithdrawn e)
    {
        view.Phase = ApplicationPhase.Withdrawn.DisplayName;
        view.WithdrawalReason = e.Reason;
    }
}