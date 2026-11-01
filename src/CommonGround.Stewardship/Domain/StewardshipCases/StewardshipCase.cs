using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.Stewardship.Domain.StewardshipCases.Events;

namespace CommonGround.Stewardship.Domain.StewardshipCases;

/// <summary>
/// Permanent stewardship file for an active ground lease. One record per ground lease,
/// opened on homeowner onboarding and closed on lease termination.
/// </summary>
public sealed class StewardshipCase : AggregateRoot<StewardshipCaseId>
{
#pragma warning disable CS8618 // State is set by Create from the stream's first event.
    private StewardshipCase() { }
#pragma warning restore CS8618

    private readonly List<OccupancyVerification> _occupancyVerifications = [];
    private readonly List<PropertyInspection> _propertyInspections = [];
    private readonly List<LeaseViolation> _leaseViolations = [];

    /// <summary>The ground lease identifier this case is associated with.</summary>
    public string GroundLeaseId { get; private set; }

    /// <summary>The current lifecycle status of the case.</summary>
    public StewardshipCaseStatus Status { get; private set; }

    /// <summary>The date the case was opened.</summary>
    public DateOnly OpenedDate { get; private set; }

    /// <summary>The date the case was closed, or null if still active.</summary>
    public DateOnly? ClosedDate { get; private set; }

    /// <summary>All occupancy verifications initiated for this case.</summary>
    public IReadOnlyList<OccupancyVerification> OccupancyVerifications => _occupancyVerifications;

    /// <summary>All property inspections scheduled for this case.</summary>
    public IReadOnlyList<PropertyInspection> PropertyInspections => _propertyInspections;

    /// <summary>All lease violations identified for this case.</summary>
    public IReadOnlyList<LeaseViolation> LeaseViolations => _leaseViolations;

    /// <summary>Opens a new stewardship case for the given ground lease.</summary>
    public static StewardshipCaseOpened Open(
        StewardshipCaseId id,
        string groundLeaseId,
        DateOnly openedDate,
        UserId openedBy)
    {
        if (string.IsNullOrWhiteSpace(groundLeaseId))
            throw new BusinessRuleViolationException("GroundLeaseId is required.");

        return new StewardshipCaseOpened(
            id.Value,
            groundLeaseId,
            openedDate,
            openedBy.Value);
    }

    /// <summary>Initiates an annual occupancy verification for the given period year.</summary>
    public OccupancyVerificationInitiated InitiateOccupancyVerification(
        int periodYear, DateOnly? surveyDeadline, UserId initiatedBy)
    {
        GuardActive();

        return new OccupancyVerificationInitiated(
            Id.Value,
            Guid.NewGuid(),
            periodYear,
            surveyDeadline,
            initiatedBy.Value);
    }

    /// <summary>Records a homeowner's occupancy survey submission.</summary>
    public OccupancyVerificationSubmitted SubmitOccupancyVerification(
        Guid verificationId,
        bool isPrimaryResident,
        bool subletDisclosed,
        string? majorChanges,
        DateOnly submittedDate)
    {
        GuardActive();
        var v = FindVerification(verificationId);
        if (v.Status is not OccupancyVerificationStatus.Pending
                     and not OccupancyVerificationStatus.Overdue)
            throw new BusinessRuleViolationException(
                "Verification must be Pending or Overdue to submit.");

        return new OccupancyVerificationSubmitted(
            Id.Value, verificationId,
            isPrimaryResident, subletDisclosed, majorChanges,
            submittedDate);
    }

    /// <summary>Confirms occupancy for a submitted verification.</summary>
    public OccupancyVerified VerifyOccupancy(Guid verificationId, DateOnly verifiedDate, UserId verifiedBy)
    {
        GuardActive();
        var v = FindVerification(verificationId);
        if (v.Status != OccupancyVerificationStatus.Submitted)
            throw new BusinessRuleViolationException(
                "Verification must be Submitted to verify.");

        return new OccupancyVerified(
            Id.Value, verificationId, verifiedDate,
            verifiedBy.Value);
    }

    /// <summary>Fails a submitted occupancy verification.</summary>
    public OccupancyVerificationFailed FailOccupancy(Guid verificationId, string reason, DateOnly failedDate, UserId recordedBy)
    {
        GuardActive();
        if (string.IsNullOrWhiteSpace(reason))
            throw new BusinessRuleViolationException("Failure reason is required.");
        var v = FindVerification(verificationId);
        if (v.Status != OccupancyVerificationStatus.Submitted)
            throw new BusinessRuleViolationException(
                "Verification must be Submitted to fail.");

        return new OccupancyVerificationFailed(
            Id.Value, verificationId, reason, failedDate,
            recordedBy.Value);
    }

    /// <summary>Marks a pending occupancy verification as overdue.</summary>
    public OccupancyVerificationOverdue MarkOccupancyVerificationOverdue(Guid verificationId, DateOnly deadline)
    {
        GuardActive();
        var v = FindVerification(verificationId);
        if (v.Status != OccupancyVerificationStatus.Pending)
            throw new BusinessRuleViolationException(
                "Only Pending verifications can become Overdue.");

        return new OccupancyVerificationOverdue(
            Id.Value, verificationId, deadline);
    }

    /// <summary>Schedules a property inspection.</summary>
    public PropertyInspectionScheduled SchedulePropertyInspection(
        DateOnly scheduledDate, string inspectorName, UserId scheduledBy)
    {
        GuardActive();
        if (string.IsNullOrWhiteSpace(inspectorName))
            throw new BusinessRuleViolationException("Inspector name is required.");

        return new PropertyInspectionScheduled(
            Id.Value, Guid.NewGuid(),
            scheduledDate, inspectorName,
            scheduledBy.Value);
    }

    /// <summary>Records the completion of a scheduled property inspection.</summary>
    public PropertyInspectionCompleted CompletePropertyInspection(
        Guid inspectionId, bool occupancyConfirmed,
        string? conditionNotes, DateOnly completedDate, UserId recordedBy)
    {
        GuardActive();
        var insp = FindInspection(inspectionId);
        if (insp.Status != PropertyInspectionStatus.Scheduled)
            throw new BusinessRuleViolationException(
                "Only Scheduled inspections can be completed.");

        return new PropertyInspectionCompleted(
            Id.Value, inspectionId, occupancyConfirmed,
            conditionNotes, completedDate,
            recordedBy.Value);
    }

    /// <summary>Cancels a scheduled property inspection.</summary>
    public PropertyInspectionCancelled CancelPropertyInspection(Guid inspectionId, string reason, UserId cancelledBy)
    {
        GuardActive();
        if (string.IsNullOrWhiteSpace(reason))
            throw new BusinessRuleViolationException("Cancellation reason is required.");
        var insp = FindInspection(inspectionId);
        if (insp.Status != PropertyInspectionStatus.Scheduled)
            throw new BusinessRuleViolationException(
                "Only Scheduled inspections can be cancelled.");

        return new PropertyInspectionCancelled(
            Id.Value, inspectionId, reason,
            cancelledBy.Value);
    }

    /// <summary>Identifies a new lease violation.</summary>
    public LeaseViolationIdentified IdentifyViolation(
        LeaseViolationType type, string description,
        DateOnly identifiedDate, UserId identifiedBy)
    {
        GuardActive();
        if (string.IsNullOrWhiteSpace(description))
            throw new BusinessRuleViolationException("Description is required.");

        return new LeaseViolationIdentified(
            Id.Value, Guid.NewGuid(), type, description,
            identifiedDate, identifiedBy.Value);
    }

    /// <summary>Issues a formal violation notice for an identified violation.</summary>
    public ViolationNoticeIssued IssueViolationNotice(
        Guid violationId, DateOnly noticeDate, DateOnly cureDeadline, UserId issuedBy)
    {
        GuardActive();
        var v = FindViolation(violationId);
        if (v.Status != LeaseViolationStatus.Identified)
            throw new BusinessRuleViolationException(
                "Notice can only be issued for Identified violations.");

        return new ViolationNoticeIssued(
            Id.Value, violationId, noticeDate, cureDeadline,
            issuedBy.Value);
    }

    /// <summary>Records the cure of a violation that has had a notice issued.</summary>
    public LeaseViolationCured CureViolation(Guid violationId, DateOnly curedDate, UserId verifiedBy)
    {
        GuardActive();
        var v = FindViolation(violationId);
        if (v.Status != LeaseViolationStatus.NoticeIssued)
            throw new BusinessRuleViolationException(
                "Only NoticeIssued violations can be cured.");

        return new LeaseViolationCured(
            Id.Value, violationId, curedDate,
            verifiedBy.Value);
    }

    /// <summary>Escalates a violation that has had a notice issued but was not cured.</summary>
    public LeaseViolationEscalated EscalateViolation(
        Guid violationId, string reason, DateOnly escalatedDate, UserId escalatedBy)
    {
        GuardActive();
        if (string.IsNullOrWhiteSpace(reason))
            throw new BusinessRuleViolationException("Escalation reason is required.");
        var v = FindViolation(violationId);
        if (v.Status != LeaseViolationStatus.NoticeIssued)
            throw new BusinessRuleViolationException(
                "Only NoticeIssued violations can be escalated.");

        return new LeaseViolationEscalated(
            Id.Value, violationId, reason, escalatedDate,
            escalatedBy.Value);
    }

    /// <summary>Closes the stewardship case.</summary>
    public StewardshipCaseClosed Close(string reason, DateOnly closedDate, UserId closedBy)
    {
        GuardActive();
        if (string.IsNullOrWhiteSpace(reason))
            throw new BusinessRuleViolationException("Close reason is required.");

        return new StewardshipCaseClosed(
            Id.Value, reason, closedDate,
            closedBy.Value);
    }

    /// <summary>Starts a stewardship case from its first event.</summary>
    public static StewardshipCase Create(StewardshipCaseOpened e)
    {
        ArgumentNullException.ThrowIfNull(e);

        var stewardshipCase = new StewardshipCase();
        stewardshipCase.Apply(e);
        return stewardshipCase;
    }

    private void Apply(StewardshipCaseOpened e)
    {
        Id = StewardshipCaseId.Create(e.AggregateId);
        GroundLeaseId = e.GroundLeaseId;
        OpenedDate = e.OpenedDate;
        Status = StewardshipCaseStatus.Active;
    }

    /// <summary>Applies <see cref="OccupancyVerificationInitiated"/>.</summary>
    public void Apply(OccupancyVerificationInitiated e)
    {
        _occupancyVerifications.Add(new OccupancyVerification
        {
            Id = e.VerificationId,
            PeriodYear = e.PeriodYear,
            SurveyDeadline = e.SurveyDeadline,
            Status = OccupancyVerificationStatus.Pending
        });
    }

    /// <summary>Applies <see cref="OccupancyVerificationSubmitted"/>.</summary>
    public void Apply(OccupancyVerificationSubmitted e)
    {
        var v = _occupancyVerifications.First(x => x.Id == e.VerificationId);
        v.IsPrimaryResident = e.IsPrimaryResident;
        v.SubletDisclosed = e.SubletDisclosed;
        v.MajorChanges = e.MajorChanges;
        v.SubmittedDate = e.SubmittedDate;
        v.Status = OccupancyVerificationStatus.Submitted;
    }

    /// <summary>Applies <see cref="OccupancyVerified"/>.</summary>
    public void Apply(OccupancyVerified e)
    {
        var v = _occupancyVerifications.First(x => x.Id == e.VerificationId);
        v.DecisionDate = e.VerifiedDate;
        v.Status = OccupancyVerificationStatus.Verified;
    }

    /// <summary>Applies <see cref="OccupancyVerificationFailed"/>.</summary>
    public void Apply(OccupancyVerificationFailed e)
    {
        var v = _occupancyVerifications.First(x => x.Id == e.VerificationId);
        v.DecisionDate = e.FailedDate;
        v.Status = OccupancyVerificationStatus.Failed;
    }

    /// <summary>Applies <see cref="OccupancyVerificationOverdue"/>.</summary>
    public void Apply(OccupancyVerificationOverdue e)
    {
        var v = _occupancyVerifications.First(x => x.Id == e.VerificationId);
        v.Status = OccupancyVerificationStatus.Overdue;
    }

    /// <summary>Applies <see cref="PropertyInspectionScheduled"/>.</summary>
    public void Apply(PropertyInspectionScheduled e)
    {
        _propertyInspections.Add(new PropertyInspection
        {
            Id = e.InspectionId,
            ScheduledDate = e.ScheduledDate,
            InspectorName = e.InspectorName,
            Status = PropertyInspectionStatus.Scheduled
        });
    }

    /// <summary>Applies <see cref="PropertyInspectionCompleted"/>.</summary>
    public void Apply(PropertyInspectionCompleted e)
    {
        var insp = _propertyInspections.First(x => x.Id == e.InspectionId);
        insp.OccupancyConfirmed = e.OccupancyConfirmed;
        insp.ConditionNotes = e.ConditionNotes;
        insp.CompletedDate = e.CompletedDate;
        insp.Status = PropertyInspectionStatus.Completed;
    }

    /// <summary>Applies <see cref="PropertyInspectionCancelled"/>.</summary>
    public void Apply(PropertyInspectionCancelled e)
    {
        var insp = _propertyInspections.First(x => x.Id == e.InspectionId);
        insp.Status = PropertyInspectionStatus.Cancelled;
    }

    /// <summary>Applies <see cref="LeaseViolationIdentified"/>.</summary>
    public void Apply(LeaseViolationIdentified e)
    {
        _leaseViolations.Add(new LeaseViolation
        {
            Id = e.ViolationId,
            Type = e.ViolationType,
            Description = e.Description,
            IdentifiedDate = e.IdentifiedDate,
            Status = LeaseViolationStatus.Identified
        });
    }

    /// <summary>Applies <see cref="ViolationNoticeIssued"/>.</summary>
    public void Apply(ViolationNoticeIssued e)
    {
        var v = _leaseViolations.First(x => x.Id == e.ViolationId);
        v.NoticeIssuedDate = e.NoticeDate;
        v.CureDeadline = e.CureDeadline;
        v.Status = LeaseViolationStatus.NoticeIssued;
    }

    /// <summary>Applies <see cref="LeaseViolationCured"/>.</summary>
    public void Apply(LeaseViolationCured e)
    {
        var v = _leaseViolations.First(x => x.Id == e.ViolationId);
        v.ResolutionDate = e.CuredDate;
        v.Status = LeaseViolationStatus.Cured;
    }

    /// <summary>Applies <see cref="LeaseViolationEscalated"/>.</summary>
    public void Apply(LeaseViolationEscalated e)
    {
        var v = _leaseViolations.First(x => x.Id == e.ViolationId);
        v.ResolutionNotes = e.Reason;
        v.ResolutionDate = e.EscalatedDate;
        v.Status = LeaseViolationStatus.Escalated;
    }

    /// <summary>Applies <see cref="StewardshipCaseClosed"/>.</summary>
    public void Apply(StewardshipCaseClosed e)
    {
        ClosedDate = e.ClosedDate;
        Status = StewardshipCaseStatus.Closed;
    }

    private void GuardActive()
    {
        if (Status != StewardshipCaseStatus.Active)
            throw new BusinessRuleViolationException(
                "Cannot modify a closed StewardshipCase.");
    }

    private OccupancyVerification FindVerification(Guid id) =>
        _occupancyVerifications.Find(v => v.Id == id)
        ?? throw new BusinessRuleViolationException(
            $"OccupancyVerification {id} not found.");

    private PropertyInspection FindInspection(Guid id) =>
        _propertyInspections.Find(i => i.Id == id)
        ?? throw new BusinessRuleViolationException(
            $"PropertyInspection {id} not found.");

    private LeaseViolation FindViolation(Guid id) =>
        _leaseViolations.Find(v => v.Id == id)
        ?? throw new BusinessRuleViolationException(
            $"LeaseViolation {id} not found.");
}
