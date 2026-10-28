using CommonGround.Stewardship.Domain.ForeclosurePreventionCases;
using CommonGround.Stewardship.Domain.ForeclosurePreventionCases.Events;
using CommonGround.Stewardship.Infrastructure.Persistence;
using JasperFx.Events;
using Marten.Events.Aggregation;

namespace CommonGround.Stewardship.Infrastructure.Projections;

/// <summary>
/// Projection that maintains the denormalized read model for foreclosure prevention cases.
/// </summary>
public sealed class ForeclosurePreventionCaseProjection : SingleStreamProjection<ForeclosurePreventionCaseView, string>
{
    /// <summary>
    /// Creates a new foreclosure prevention case view from the case opened event.
    /// </summary>
    public static ForeclosurePreventionCaseView Create(IEvent<ForeclosurePreventionCaseOpened> e) => new()
    {
        Id = e.Data.AggregateId,
        GroundLeaseId = e.Data.GroundLeaseId,
        Trigger = e.Data.Trigger.ToString(),
        Status = nameof(ForeclosurePreventionCaseStatus.Opened),
        OpenedDate = e.Data.OpenedDate
    };

    /// <summary>
    /// Applies a servicer delinquency recorded event to the view.
    /// </summary>
    public static void Apply(ForeclosurePreventionCaseView view, ServicerDelinquencyRecorded e)
    {
        view.ServicerDelinquencies.Add(new ServicerDelinquencyRecord
        {
            ServicerName = e.ServicerName,
            DelinquentMonths = e.DelinquentMonths,
            RecordedDate = e.RecordedDate
        });
    }

    /// <summary>
    /// Applies a counseling referral made event to the view.
    /// </summary>
    public static void Apply(ForeclosurePreventionCaseView view, CounselingReferralMade e)
    {
        view.CounselingReferrals.Add(new CounselingReferralRecord
        {
            Id = e.ReferralId.ToString(),
            AgencyName = e.AgencyName,
            ReferralDate = e.ReferralDate
        });
        if (view.Status == nameof(ForeclosurePreventionCaseStatus.Opened))
            view.Status = nameof(ForeclosurePreventionCaseStatus.Active);
    }

    /// <summary>
    /// Applies a counseling outcome recorded event to the view.
    /// </summary>
    public static void Apply(ForeclosurePreventionCaseView view, CounselingOutcomeRecorded e)
    {
        var referral = view.CounselingReferrals.First(r => r.Id == e.ReferralId.ToString());
        referral.Outcome = e.Outcome;
        referral.OutcomeDate = e.OutcomeDate;
    }

    /// <summary>
    /// Applies a forbearance requested event to the view.
    /// </summary>
    public static void Apply(ForeclosurePreventionCaseView view, ForbearanceRequested e)
    {
        view.ForbearanceRequests.Add(new ForbearanceRequestRecord
        {
            Id = e.RequestId.ToString(),
            LenderName = e.LenderName,
            RequestedDate = e.RequestedDate
        });
        if (view.Status == nameof(ForeclosurePreventionCaseStatus.Opened))
            view.Status = nameof(ForeclosurePreventionCaseStatus.Active);
    }

    /// <summary>
    /// Applies a forbearance outcome recorded event to the view.
    /// </summary>
    public static void Apply(ForeclosurePreventionCaseView view, ForbearanceOutcomeRecorded e)
    {
        var request = view.ForbearanceRequests.First(r => r.Id == e.RequestId.ToString());
        request.Outcome = e.Outcome.ToString();
        request.OutcomeDate = e.OutcomeDate;
        request.Notes = e.Notes;
    }

    /// <summary>
    /// Applies a cure period activated event to the view.
    /// </summary>
    public static void Apply(ForeclosurePreventionCaseView view, CurePeriodActivated e)
    {
        view.CurePeriodActivations.Add(new CurePeriodActivationRecord
        {
            Id = e.ActivationId.ToString(),
            LraId = e.LraId,
            ActivationDate = e.ActivationDate,
            CurePeriodDays = e.CurePeriodDays,
            CureDeadline = e.CureDeadline
        });
        if (view.Status == nameof(ForeclosurePreventionCaseStatus.Opened))
            view.Status = nameof(ForeclosurePreventionCaseStatus.Active);
    }

    /// <summary>
    /// Applies a foreclosure prevention case resolved event to the view.
    /// </summary>
    public static void Apply(ForeclosurePreventionCaseView view, ForeclosurePreventionCaseResolved e)
    {
        view.Outcome = e.Outcome.ToString();
        view.ResolvedDate = e.ResolvedDate;
        view.Status = nameof(ForeclosurePreventionCaseStatus.Resolved);
    }
}
