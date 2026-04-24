using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.Stewardship.Domain.StewardshipCases.ChildEntities;
using CommonGround.Stewardship.Domain.StewardshipCases.Events;

namespace CommonGround.Stewardship.Domain.StewardshipCases;

/// <summary>
/// Permanent stewardship file for an active ground lease. One record per ground lease,
/// opened on homeowner onboarding and closed on lease termination.
/// </summary>
public sealed class StewardshipCase : AggregateRoot<StewardshipCaseId>
{
#pragma warning disable CS8618
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
    public static StewardshipCase Open(
        string groundLeaseId,
        DateOnly openedDate,
        UserId openedBy)
    {
        if (string.IsNullOrWhiteSpace(groundLeaseId))
            throw new BusinessRuleViolationException("GroundLeaseId is required.");
        ArgumentNullException.ThrowIfNull(openedBy);

        var sc = new StewardshipCase();
        sc.ApplyChange(new StewardshipCaseOpened(
            StewardshipCaseId.Generate().Value,
            groundLeaseId,
            openedDate,
            openedBy.Value,
            DateTimeOffset.UtcNow));
        return sc;
    }

    /// <summary>Initiates an annual occupancy verification for the given period year.</summary>
    public void InitiateOccupancyVerification(
        int periodYear, DateOnly? surveyDeadline, UserId initiatedBy)
    {
        GuardActive();
        ArgumentNullException.ThrowIfNull(initiatedBy);

        ApplyChange(new OccupancyVerificationInitiated(
            Id.Value,
            Guid.NewGuid(),
            periodYear,
            surveyDeadline,
            initiatedBy.Value,
            DateTimeOffset.UtcNow));
    }

    /// <summary>Records a homeowner's occupancy survey submission.</summary>
    public void SubmitOccupancyVerification(
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

        ApplyChange(new OccupancyVerificationSubmitted(
            Id.Value, verificationId,
            isPrimaryResident, subletDisclosed, majorChanges,
            submittedDate, DateTimeOffset.UtcNow));
    }

    /// <summary>Confirms occupancy for a submitted verification.</summary>
    public void VerifyOccupancy(Guid verificationId, DateOnly verifiedDate, UserId verifiedBy)
    {
        GuardActive();
        ArgumentNullException.ThrowIfNull(verifiedBy);
        var v = FindVerification(verificationId);
        if (v.Status != OccupancyVerificationStatus.Submitted)
            throw new BusinessRuleViolationException(
                "Verification must be Submitted to verify.");

        ApplyChange(new OccupancyVerified(
            Id.Value, verificationId, verifiedDate,
            verifiedBy.Value, DateTimeOffset.UtcNow));
    }

    /// <summary>Fails a submitted occupancy verification.</summary>
    public void FailOccupancy(Guid verificationId, string reason, DateOnly failedDate, UserId recordedBy)
    {
        GuardActive();
        ArgumentNullException.ThrowIfNull(recordedBy);
        if (string.IsNullOrWhiteSpace(reason))
            throw new BusinessRuleViolationException("Failure reason is required.");
        var v = FindVerification(verificationId);
        if (v.Status != OccupancyVerificationStatus.Submitted)
            throw new BusinessRuleViolationException(
                "Verification must be Submitted to fail.");

        ApplyChange(new OccupancyVerificationFailed(
            Id.Value, verificationId, reason, failedDate,
            recordedBy.Value, DateTimeOffset.UtcNow));
    }

    /// <summary>Marks a pending occupancy verification as overdue.</summary>
    public void MarkOccupancyVerificationOverdue(Guid verificationId, DateOnly deadline)
    {
        GuardActive();
        var v = FindVerification(verificationId);
        if (v.Status != OccupancyVerificationStatus.Pending)
            throw new BusinessRuleViolationException(
                "Only Pending verifications can become Overdue.");

        ApplyChange(new OccupancyVerificationOverdue(
            Id.Value, verificationId, deadline, DateTimeOffset.UtcNow));
    }

    /// <summary>Schedules a property inspection.</summary>
    public void SchedulePropertyInspection(
        DateOnly scheduledDate, string inspectorName, UserId scheduledBy)
    {
        GuardActive();
        ArgumentNullException.ThrowIfNull(scheduledBy);
        if (string.IsNullOrWhiteSpace(inspectorName))
            throw new BusinessRuleViolationException("Inspector name is required.");

        ApplyChange(new PropertyInspectionScheduled(
            Id.Value, Guid.NewGuid(),
            scheduledDate, inspectorName,
            scheduledBy.Value, DateTimeOffset.UtcNow));
    }

    /// <summary>Records the completion of a scheduled property inspection.</summary>
    public void CompletePropertyInspection(
        Guid inspectionId, bool occupancyConfirmed,
        string? conditionNotes, DateOnly completedDate, UserId recordedBy)
    {
        GuardActive();
        ArgumentNullException.ThrowIfNull(recordedBy);
        var insp = FindInspection(inspectionId);
        if (insp.Status != PropertyInspectionStatus.Scheduled)
            throw new BusinessRuleViolationException(
                "Only Scheduled inspections can be completed.");

        ApplyChange(new PropertyInspectionCompleted(
            Id.Value, inspectionId, occupancyConfirmed,
            conditionNotes, completedDate,
            recordedBy.Value, DateTimeOffset.UtcNow));
    }

    /// <summary>Cancels a scheduled property inspection.</summary>
    public void CancelPropertyInspection(Guid inspectionId, string reason, UserId cancelledBy)
    {
        GuardActive();
        ArgumentNullException.ThrowIfNull(cancelledBy);
        if (string.IsNullOrWhiteSpace(reason))
            throw new BusinessRuleViolationException("Cancellation reason is required.");
        var insp = FindInspection(inspectionId);
        if (insp.Status != PropertyInspectionStatus.Scheduled)
            throw new BusinessRuleViolationException(
                "Only Scheduled inspections can be cancelled.");

        ApplyChange(new PropertyInspectionCancelled(
            Id.Value, inspectionId, reason,
            cancelledBy.Value, DateTimeOffset.UtcNow));
    }

    /// <summary>Identifies a new lease violation.</summary>
    public void IdentifyViolation(
        LeaseViolationType type, string description,
        DateOnly identifiedDate, UserId identifiedBy)
    {
        GuardActive();
        ArgumentNullException.ThrowIfNull(identifiedBy);
        if (string.IsNullOrWhiteSpace(description))
            throw new BusinessRuleViolationException("Description is required.");

        ApplyChange(new LeaseViolationIdentified(
            Id.Value, Guid.NewGuid(), type, description,
            identifiedDate, identifiedBy.Value, DateTimeOffset.UtcNow));
    }

    /// <summary>Issues a formal violation notice for an identified violation.</summary>
    public void IssueViolationNotice(
        Guid violationId, DateOnly noticeDate, DateOnly cureDeadline, UserId issuedBy)
    {
        GuardActive();
        ArgumentNullException.ThrowIfNull(issuedBy);
        var v = FindViolation(violationId);
        if (v.Status != LeaseViolationStatus.Identified)
            throw new BusinessRuleViolationException(
                "Notice can only be issued for Identified violations.");

        ApplyChange(new ViolationNoticeIssued(
            Id.Value, violationId, noticeDate, cureDeadline,
            issuedBy.Value, DateTimeOffset.UtcNow));
    }

    /// <summary>Records the cure of a violation that has had a notice issued.</summary>
    public void CureViolation(Guid violationId, DateOnly curedDate, UserId verifiedBy)
    {
        GuardActive();
        ArgumentNullException.ThrowIfNull(verifiedBy);
        var v = FindViolation(violationId);
        if (v.Status != LeaseViolationStatus.NoticeIssued)
            throw new BusinessRuleViolationException(
                "Only NoticeIssued violations can be cured.");

        ApplyChange(new LeaseViolationCured(
            Id.Value, violationId, curedDate,
            verifiedBy.Value, DateTimeOffset.UtcNow));
    }

    /// <summary>Escalates a violation that has had a notice issued but was not cured.</summary>
    public void EscalateViolation(
        Guid violationId, string reason, DateOnly escalatedDate, UserId escalatedBy)
    {
        GuardActive();
        ArgumentNullException.ThrowIfNull(escalatedBy);
        if (string.IsNullOrWhiteSpace(reason))
            throw new BusinessRuleViolationException("Escalation reason is required.");
        var v = FindViolation(violationId);
        if (v.Status != LeaseViolationStatus.NoticeIssued)
            throw new BusinessRuleViolationException(
                "Only NoticeIssued violations can be escalated.");

        ApplyChange(new LeaseViolationEscalated(
            Id.Value, violationId, reason, escalatedDate,
            escalatedBy.Value, DateTimeOffset.UtcNow));
    }

    /// <summary>Closes the stewardship case.</summary>
    public void Close(string reason, DateOnly closedDate, UserId closedBy)
    {
        GuardActive();
        ArgumentNullException.ThrowIfNull(closedBy);
        if (string.IsNullOrWhiteSpace(reason))
            throw new BusinessRuleViolationException("Close reason is required.");

        ApplyChange(new StewardshipCaseClosed(
            Id.Value, reason, closedDate,
            closedBy.Value, DateTimeOffset.UtcNow));
    }

    /// <inheritdoc/>
    protected override void ApplyEvent(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case StewardshipCaseOpened e: Apply(e); break;
            case OccupancyVerificationInitiated e: Apply(e); break;
            case OccupancyVerificationSubmitted e: Apply(e); break;
            case OccupancyVerified e: Apply(e); break;
            case OccupancyVerificationFailed e: Apply(e); break;
            case OccupancyVerificationOverdue e: Apply(e); break;
            case PropertyInspectionScheduled e: Apply(e); break;
            case PropertyInspectionCompleted e: Apply(e); break;
            case PropertyInspectionCancelled e: Apply(e); break;
            case LeaseViolationIdentified e: Apply(e); break;
            case ViolationNoticeIssued e: Apply(e); break;
            case LeaseViolationCured e: Apply(e); break;
            case LeaseViolationEscalated e: Apply(e); break;
            case StewardshipCaseClosed e: Apply(e); break;
            default: throw new InvalidOperationException(
                $"Unknown event: {domainEvent?.GetType().Name}");
        }
    }

    private void Apply(StewardshipCaseOpened e)
    {
        Id = StewardshipCaseId.Create(e.AggregateId);
        GroundLeaseId = e.GroundLeaseId;
        OpenedDate = e.OpenedDate;
        Status = StewardshipCaseStatus.Active;
    }

    private void Apply(OccupancyVerificationInitiated e)
    {
        _occupancyVerifications.Add(new OccupancyVerification
        {
            Id = e.VerificationId,
            PeriodYear = e.PeriodYear,
            SurveyDeadline = e.SurveyDeadline,
            Status = OccupancyVerificationStatus.Pending
        });
    }

    private void Apply(OccupancyVerificationSubmitted e)
    {
        var v = _occupancyVerifications.First(x => x.Id == e.VerificationId);
        v.IsPrimaryResident = e.IsPrimaryResident;
        v.SubletDisclosed = e.SubletDisclosed;
        v.MajorChanges = e.MajorChanges;
        v.SubmittedDate = e.SubmittedDate;
        v.Status = OccupancyVerificationStatus.Submitted;
    }

    private void Apply(OccupancyVerified e)
    {
        var v = _occupancyVerifications.First(x => x.Id == e.VerificationId);
        v.DecisionDate = e.VerifiedDate;
        v.Status = OccupancyVerificationStatus.Verified;
    }

    private void Apply(OccupancyVerificationFailed e)
    {
        var v = _occupancyVerifications.First(x => x.Id == e.VerificationId);
        v.DecisionDate = e.FailedDate;
        v.Status = OccupancyVerificationStatus.Failed;
    }

    private void Apply(OccupancyVerificationOverdue e)
    {
        var v = _occupancyVerifications.First(x => x.Id == e.VerificationId);
        v.Status = OccupancyVerificationStatus.Overdue;
    }

    private void Apply(PropertyInspectionScheduled e)
    {
        _propertyInspections.Add(new PropertyInspection
        {
            Id = e.InspectionId,
            ScheduledDate = e.ScheduledDate,
            InspectorName = e.InspectorName,
            Status = PropertyInspectionStatus.Scheduled
        });
    }

    private void Apply(PropertyInspectionCompleted e)
    {
        var insp = _propertyInspections.First(x => x.Id == e.InspectionId);
        insp.OccupancyConfirmed = e.OccupancyConfirmed;
        insp.ConditionNotes = e.ConditionNotes;
        insp.CompletedDate = e.CompletedDate;
        insp.Status = PropertyInspectionStatus.Completed;
    }

    private void Apply(PropertyInspectionCancelled e)
    {
        var insp = _propertyInspections.First(x => x.Id == e.InspectionId);
        insp.Status = PropertyInspectionStatus.Cancelled;
    }

    private void Apply(LeaseViolationIdentified e)
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

    private void Apply(ViolationNoticeIssued e)
    {
        var v = _leaseViolations.First(x => x.Id == e.ViolationId);
        v.NoticeIssuedDate = e.NoticeDate;
        v.CureDeadline = e.CureDeadline;
        v.Status = LeaseViolationStatus.NoticeIssued;
    }

    private void Apply(LeaseViolationCured e)
    {
        var v = _leaseViolations.First(x => x.Id == e.ViolationId);
        v.ResolutionDate = e.CuredDate;
        v.Status = LeaseViolationStatus.Cured;
    }

    private void Apply(LeaseViolationEscalated e)
    {
        var v = _leaseViolations.First(x => x.Id == e.ViolationId);
        v.ResolutionNotes = e.Reason;
        v.ResolutionDate = e.EscalatedDate;
        v.Status = LeaseViolationStatus.Escalated;
    }

    private void Apply(StewardshipCaseClosed e)
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
