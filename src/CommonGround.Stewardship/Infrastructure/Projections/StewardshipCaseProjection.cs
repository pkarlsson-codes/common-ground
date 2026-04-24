using CommonGround.Stewardship.Domain.StewardshipCases;
using CommonGround.Stewardship.Domain.StewardshipCases.Events;
using CommonGround.Stewardship.Infrastructure.Persistence;
using JasperFx.Events;
using Marten.Events.Aggregation;

namespace CommonGround.Stewardship.Infrastructure.Projections;

/// <summary>
/// Projection that maintains the denormalized read model for stewardship cases.
/// </summary>
public sealed class StewardshipCaseProjection : SingleStreamProjection<StewardshipCaseView, string>
{
    /// <summary>
    /// Creates a new stewardship case view from the case opened event.
    /// </summary>
    public static StewardshipCaseView Create(IEvent<StewardshipCaseOpened> e) => new()
    {
        Id = e.Data.AggregateId,
        GroundLeaseId = e.Data.GroundLeaseId,
        Status = nameof(StewardshipCaseStatus.Active),
        OpenedDate = e.Data.OpenedDate
    };

    /// <summary>
    /// Applies an occupancy verification initiated event to the view.
    /// </summary>
    public static void Apply(StewardshipCaseView view, OccupancyVerificationInitiated e)
    {
        view.OccupancyVerifications.Add(new OccupancyVerificationRecord
        {
            Id = e.VerificationId.ToString(),
            PeriodYear = e.PeriodYear,
            SurveyDeadline = e.SurveyDeadline,
            Status = nameof(OccupancyVerificationStatus.Pending)
        });
    }

    /// <summary>
    /// Applies an occupancy verification submitted event to the view.
    /// </summary>
    public static void Apply(StewardshipCaseView view, OccupancyVerificationSubmitted e)
    {
        var v = view.OccupancyVerifications.First(x => x.Id == e.VerificationId.ToString());
        v.IsPrimaryResident = e.IsPrimaryResident;
        v.SubletDisclosed = e.SubletDisclosed;
        v.MajorChanges = e.MajorChanges;
        v.SubmittedDate = e.SubmittedDate;
        v.Status = nameof(OccupancyVerificationStatus.Submitted);
    }

    /// <summary>
    /// Applies an occupancy verified event to the view.
    /// </summary>
    public static void Apply(StewardshipCaseView view, OccupancyVerified e)
    {
        var v = view.OccupancyVerifications.First(x => x.Id == e.VerificationId.ToString());
        v.DecisionDate = e.VerifiedDate;
        v.Status = nameof(OccupancyVerificationStatus.Verified);
    }

    /// <summary>
    /// Applies an occupancy verification failed event to the view.
    /// </summary>
    public static void Apply(StewardshipCaseView view, OccupancyVerificationFailed e)
    {
        var v = view.OccupancyVerifications.First(x => x.Id == e.VerificationId.ToString());
        v.DecisionDate = e.FailedDate;
        v.Reason = e.Reason;
        v.Status = nameof(OccupancyVerificationStatus.Failed);
    }

    /// <summary>
    /// Applies an occupancy verification overdue event to the view.
    /// </summary>
    public static void Apply(StewardshipCaseView view, OccupancyVerificationOverdue e)
    {
        var v = view.OccupancyVerifications.First(x => x.Id == e.VerificationId.ToString());
        v.Status = nameof(OccupancyVerificationStatus.Overdue);
    }

    /// <summary>
    /// Applies a property inspection scheduled event to the view.
    /// </summary>
    public static void Apply(StewardshipCaseView view, PropertyInspectionScheduled e)
    {
        view.PropertyInspections.Add(new PropertyInspectionRecord
        {
            Id = e.InspectionId.ToString(),
            ScheduledDate = e.ScheduledDate,
            InspectorName = e.InspectorName,
            Status = nameof(PropertyInspectionStatus.Scheduled)
        });
    }

    /// <summary>
    /// Applies a property inspection completed event to the view.
    /// </summary>
    public static void Apply(StewardshipCaseView view, PropertyInspectionCompleted e)
    {
        var insp = view.PropertyInspections.First(x => x.Id == e.InspectionId.ToString());
        insp.OccupancyConfirmed = e.OccupancyConfirmed;
        insp.ConditionNotes = e.ConditionNotes;
        insp.CompletedDate = e.CompletedDate;
        insp.Status = nameof(PropertyInspectionStatus.Completed);
    }

    /// <summary>
    /// Applies a property inspection cancelled event to the view.
    /// </summary>
    public static void Apply(StewardshipCaseView view, PropertyInspectionCancelled e)
    {
        var insp = view.PropertyInspections.First(x => x.Id == e.InspectionId.ToString());
        insp.CancellationReason = e.Reason;
        insp.Status = nameof(PropertyInspectionStatus.Cancelled);
    }

    /// <summary>
    /// Applies a lease violation identified event to the view.
    /// </summary>
    public static void Apply(StewardshipCaseView view, LeaseViolationIdentified e)
    {
        view.LeaseViolations.Add(new LeaseViolationRecord
        {
            Id = e.ViolationId.ToString(),
            ViolationType = e.ViolationType.ToString(),
            Description = e.Description,
            IdentifiedDate = e.IdentifiedDate,
            Status = nameof(LeaseViolationStatus.Identified)
        });
    }

    /// <summary>
    /// Applies a violation notice issued event to the view.
    /// </summary>
    public static void Apply(StewardshipCaseView view, ViolationNoticeIssued e)
    {
        var v = view.LeaseViolations.First(x => x.Id == e.ViolationId.ToString());
        v.NoticeIssuedDate = e.NoticeDate;
        v.CureDeadline = e.CureDeadline;
        v.Status = nameof(LeaseViolationStatus.NoticeIssued);
    }

    /// <summary>
    /// Applies a lease violation cured event to the view.
    /// </summary>
    public static void Apply(StewardshipCaseView view, LeaseViolationCured e)
    {
        var v = view.LeaseViolations.First(x => x.Id == e.ViolationId.ToString());
        v.ResolutionDate = e.CuredDate;
        v.Status = nameof(LeaseViolationStatus.Cured);
    }

    /// <summary>
    /// Applies a lease violation escalated event to the view.
    /// </summary>
    public static void Apply(StewardshipCaseView view, LeaseViolationEscalated e)
    {
        var v = view.LeaseViolations.First(x => x.Id == e.ViolationId.ToString());
        v.ResolutionNotes = e.Reason;
        v.ResolutionDate = e.EscalatedDate;
        v.Status = nameof(LeaseViolationStatus.Escalated);
    }

    /// <summary>
    /// Applies a stewardship case closed event to the view.
    /// </summary>
    public static void Apply(StewardshipCaseView view, StewardshipCaseClosed e)
    {
        view.ClosedDate = e.ClosedDate;
        view.Status = nameof(StewardshipCaseStatus.Closed);
    }
}
