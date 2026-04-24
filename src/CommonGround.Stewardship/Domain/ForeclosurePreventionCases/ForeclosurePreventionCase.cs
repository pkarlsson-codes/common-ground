using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.Stewardship.Domain.ForeclosurePreventionCases.ChildEntities;
using CommonGround.Stewardship.Domain.ForeclosurePreventionCases.Events;

namespace CommonGround.Stewardship.Domain.ForeclosurePreventionCases;

/// <summary>
/// Tracks CLT intervention efforts when a homeowner faces risk of foreclosure.
/// Opened by lease fee arrears escalation or a manually recorded servicer delinquency.
/// </summary>
public sealed class ForeclosurePreventionCase : AggregateRoot<ForeclosurePreventionCaseId>
{
#pragma warning disable CS8618
    private ForeclosurePreventionCase() { }
#pragma warning restore CS8618

    private readonly List<CounselingReferral> _counselingReferrals = [];
    private readonly List<ForbearanceRequest> _forbearanceRequests = [];
    private readonly List<CurePeriodActivation> _curePeriodActivations = [];

    /// <summary>The ground lease identifier this case is associated with.</summary>
    public string GroundLeaseId { get; private set; }

    /// <summary>The event that triggered this case.</summary>
    public ForeclosurePreventionTrigger Trigger { get; private set; }

    /// <summary>The current lifecycle status of the case.</summary>
    public ForeclosurePreventionCaseStatus Status { get; private set; }

    /// <summary>The outcome when the case is resolved, or null if not yet resolved.</summary>
    public ForeclosurePreventionOutcome? Outcome { get; private set; }

    /// <summary>The date the case was opened.</summary>
    public DateOnly OpenedDate { get; private set; }

    /// <summary>The date the case was resolved, or null if still active.</summary>
    public DateOnly? ResolvedDate { get; private set; }

    /// <summary>All counseling referrals made for this case.</summary>
    public IReadOnlyList<CounselingReferral> CounselingReferrals => _counselingReferrals;

    /// <summary>All forbearance requests submitted for this case.</summary>
    public IReadOnlyList<ForbearanceRequest> ForbearanceRequests => _forbearanceRequests;

    /// <summary>All cure period activations for this case.</summary>
    public IReadOnlyList<CurePeriodActivation> CurePeriodActivations => _curePeriodActivations;

    /// <summary>Opens a new foreclosure prevention case.</summary>
    public static ForeclosurePreventionCase Open(
        string groundLeaseId,
        ForeclosurePreventionTrigger trigger,
        DateOnly openedDate,
        UserId openedBy)
    {
        if (string.IsNullOrWhiteSpace(groundLeaseId))
            throw new BusinessRuleViolationException("GroundLeaseId is required.");
        ArgumentNullException.ThrowIfNull(openedBy);

        var fpc = new ForeclosurePreventionCase();
        fpc.ApplyChange(new ForeclosurePreventionCaseOpened(
            ForeclosurePreventionCaseId.Generate().Value,
            groundLeaseId, trigger, openedDate,
            openedBy.Value, DateTimeOffset.UtcNow));
        return fpc;
    }

    /// <summary>Records a servicer delinquency without advancing the case to Active.</summary>
    public void RecordServicerDelinquency(
        string servicerName, int delinquentMonths, DateOnly recordedDate, UserId recordedBy)
    {
        GuardNotResolved();
        ArgumentNullException.ThrowIfNull(recordedBy);
        if (string.IsNullOrWhiteSpace(servicerName))
            throw new BusinessRuleViolationException("Servicer name is required.");

        ApplyChange(new ServicerDelinquencyRecorded(
            Id.Value, servicerName, delinquentMonths, recordedDate,
            recordedBy.Value, DateTimeOffset.UtcNow));
    }

    /// <summary>Makes a housing counseling referral, advancing the case to Active if still Opened.</summary>
    public void MakeCounselingReferral(
        string agencyName, DateOnly referralDate, UserId referredBy)
    {
        GuardNotResolved();
        ArgumentNullException.ThrowIfNull(referredBy);
        if (string.IsNullOrWhiteSpace(agencyName))
            throw new BusinessRuleViolationException("Agency name is required.");

        ApplyChange(new CounselingReferralMade(
            Id.Value, Guid.NewGuid(), agencyName, referralDate,
            referredBy.Value, DateTimeOffset.UtcNow));
    }

    /// <summary>Records the outcome of a counseling referral.</summary>
    public void RecordCounselingOutcome(
        Guid referralId, string outcome, DateOnly outcomeDate, UserId recordedBy)
    {
        GuardActive();
        ArgumentNullException.ThrowIfNull(recordedBy);
        if (string.IsNullOrWhiteSpace(outcome))
            throw new BusinessRuleViolationException("Outcome is required.");
        var referral = FindReferral(referralId);
        _ = referral;

        ApplyChange(new CounselingOutcomeRecorded(
            Id.Value, referralId, outcome, outcomeDate,
            recordedBy.Value, DateTimeOffset.UtcNow));
    }

    /// <summary>Submits a forbearance request to a lender, advancing the case to Active if still Opened.</summary>
    public void RequestForbearance(
        string lenderName, DateOnly requestedDate, UserId requestedBy)
    {
        GuardNotResolved();
        ArgumentNullException.ThrowIfNull(requestedBy);
        if (string.IsNullOrWhiteSpace(lenderName))
            throw new BusinessRuleViolationException("Lender name is required.");

        ApplyChange(new ForbearanceRequested(
            Id.Value, Guid.NewGuid(), lenderName, requestedDate,
            requestedBy.Value, DateTimeOffset.UtcNow));
    }

    /// <summary>Records the lender's decision on a forbearance request.</summary>
    public void RecordForbearanceOutcome(
        Guid requestId, ForbearanceOutcome outcome, DateOnly outcomeDate,
        string? notes, UserId recordedBy)
    {
        GuardActive();
        ArgumentNullException.ThrowIfNull(recordedBy);
        var request = FindForbearanceRequest(requestId);
        _ = request;

        ApplyChange(new ForbearanceOutcomeRecorded(
            Id.Value, requestId, outcome, outcomeDate, notes,
            recordedBy.Value, DateTimeOffset.UtcNow));
    }

    /// <summary>Activates a CLT cure period under a lender recognition agreement, advancing the case to Active if still Opened.</summary>
    public void ActivateCurePeriod(
        string lraId, DateOnly activationDate, int curePeriodDays, UserId activatedBy)
    {
        GuardNotResolved();
        ArgumentNullException.ThrowIfNull(activatedBy);
        if (string.IsNullOrWhiteSpace(lraId))
            throw new BusinessRuleViolationException("LraId is required.");
        if (curePeriodDays <= 0)
            throw new BusinessRuleViolationException("Cure period days must be positive.");

        var cureDeadline = activationDate.AddDays(curePeriodDays);
        ApplyChange(new CurePeriodActivated(
            Id.Value, Guid.NewGuid(), lraId, activationDate, curePeriodDays, cureDeadline,
            activatedBy.Value, DateTimeOffset.UtcNow));
    }

    /// <summary>Resolves the case with the given outcome.</summary>
    public void Resolve(ForeclosurePreventionOutcome outcome, DateOnly resolvedDate, UserId resolvedBy)
    {
        if (Status != ForeclosurePreventionCaseStatus.Active)
            throw new BusinessRuleViolationException(
                "ForeclosurePreventionCase must be Active to resolve.");
        ArgumentNullException.ThrowIfNull(resolvedBy);

        ApplyChange(new ForeclosurePreventionCaseResolved(
            Id.Value, outcome, resolvedDate,
            resolvedBy.Value, DateTimeOffset.UtcNow));
    }

    /// <inheritdoc/>
    protected override void ApplyEvent(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case ForeclosurePreventionCaseOpened e: Apply(e); break;
            case ServicerDelinquencyRecorded: break;
            case CounselingReferralMade e: Apply(e); break;
            case CounselingOutcomeRecorded e: Apply(e); break;
            case ForbearanceRequested e: Apply(e); break;
            case ForbearanceOutcomeRecorded e: Apply(e); break;
            case CurePeriodActivated e: Apply(e); break;
            case ForeclosurePreventionCaseResolved e: Apply(e); break;
            default: throw new InvalidOperationException(
                $"Unknown event: {domainEvent?.GetType().Name}");
        }
    }

    private void Apply(ForeclosurePreventionCaseOpened e)
    {
        Id = ForeclosurePreventionCaseId.Create(e.AggregateId);
        GroundLeaseId = e.GroundLeaseId;
        Trigger = e.Trigger;
        OpenedDate = e.OpenedDate;
        Status = ForeclosurePreventionCaseStatus.Opened;
    }

    private void Apply(CounselingReferralMade e)
    {
        _counselingReferrals.Add(new CounselingReferral
        {
            Id = e.ReferralId,
            AgencyName = e.AgencyName,
            ReferralDate = e.ReferralDate
        });
        if (Status == ForeclosurePreventionCaseStatus.Opened)
            Status = ForeclosurePreventionCaseStatus.Active;
    }

    private void Apply(CounselingOutcomeRecorded e)
    {
        var referral = _counselingReferrals.First(r => r.Id == e.ReferralId);
        referral.Outcome = e.Outcome;
        referral.OutcomeDate = e.OutcomeDate;
    }

    private void Apply(ForbearanceRequested e)
    {
        _forbearanceRequests.Add(new ForbearanceRequest
        {
            Id = e.RequestId,
            LenderName = e.LenderName,
            RequestedDate = e.RequestedDate
        });
        if (Status == ForeclosurePreventionCaseStatus.Opened)
            Status = ForeclosurePreventionCaseStatus.Active;
    }

    private void Apply(ForbearanceOutcomeRecorded e)
    {
        var request = _forbearanceRequests.First(r => r.Id == e.RequestId);
        request.Outcome = e.Outcome;
        request.OutcomeDate = e.OutcomeDate;
        request.Notes = e.Notes;
    }

    private void Apply(CurePeriodActivated e)
    {
        _curePeriodActivations.Add(new CurePeriodActivation
        {
            Id = e.ActivationId,
            LraId = e.LraId,
            ActivationDate = e.ActivationDate,
            CurePeriodDays = e.CurePeriodDays,
            CureDeadline = e.CureDeadline
        });
        if (Status == ForeclosurePreventionCaseStatus.Opened)
            Status = ForeclosurePreventionCaseStatus.Active;
    }

    private void Apply(ForeclosurePreventionCaseResolved e)
    {
        Outcome = e.Outcome;
        ResolvedDate = e.ResolvedDate;
        Status = ForeclosurePreventionCaseStatus.Resolved;
    }

    private void GuardNotResolved()
    {
        if (Status == ForeclosurePreventionCaseStatus.Resolved)
            throw new BusinessRuleViolationException(
                "Cannot modify a resolved ForeclosurePreventionCase.");
    }

    private void GuardActive()
    {
        if (Status != ForeclosurePreventionCaseStatus.Active)
            throw new BusinessRuleViolationException(
                "ForeclosurePreventionCase must be Active for this operation.");
    }

    private CounselingReferral FindReferral(Guid id) =>
        _counselingReferrals.Find(r => r.Id == id)
        ?? throw new BusinessRuleViolationException(
            $"CounselingReferral {id} not found.");

    private ForbearanceRequest FindForbearanceRequest(Guid id) =>
        _forbearanceRequests.Find(r => r.Id == id)
        ?? throw new BusinessRuleViolationException(
            $"ForbearanceRequest {id} not found.");
}
