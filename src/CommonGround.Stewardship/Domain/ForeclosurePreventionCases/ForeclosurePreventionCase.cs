using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.Stewardship.Domain.ForeclosurePreventionCases.Events;

namespace CommonGround.Stewardship.Domain.ForeclosurePreventionCases;

/// <summary>
/// Tracks CLT intervention efforts when a homeowner faces risk of foreclosure.
/// Opened by lease fee arrears escalation or a manually recorded servicer delinquency.
/// </summary>
public sealed class ForeclosurePreventionCase : AggregateRoot<ForeclosurePreventionCaseId>
{
#pragma warning disable CS8618 // State is set by Create from the stream's first event.
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
    public static ForeclosurePreventionCaseOpened Open(
        ForeclosurePreventionCaseId id,
        string groundLeaseId,
        ForeclosurePreventionTrigger trigger,
        DateOnly openedDate,
        UserId openedBy)
    {
        if (string.IsNullOrWhiteSpace(groundLeaseId))
            throw new BusinessRuleViolationException("GroundLeaseId is required.");

        return new ForeclosurePreventionCaseOpened(
            id.Value,
            groundLeaseId, trigger, openedDate,
            openedBy.Value);
    }

    /// <summary>Records a servicer delinquency without advancing the case to Active.</summary>
    public ServicerDelinquencyRecorded RecordServicerDelinquency(
        string servicerName, int delinquentMonths, DateOnly recordedDate, UserId recordedBy)
    {
        GuardNotResolved();
        if (string.IsNullOrWhiteSpace(servicerName))
            throw new BusinessRuleViolationException("Servicer name is required.");
        if (delinquentMonths <= 0)
            throw new BusinessRuleViolationException("Delinquent months must be positive.");

        return new ServicerDelinquencyRecorded(
            Id.Value, servicerName, delinquentMonths, recordedDate,
            recordedBy.Value);
    }

    /// <summary>Makes a housing counseling referral, advancing the case to Active if still Opened.</summary>
    public CounselingReferralMade MakeCounselingReferral(
        string agencyName, DateOnly referralDate, UserId referredBy)
    {
        GuardNotResolved();
        if (string.IsNullOrWhiteSpace(agencyName))
            throw new BusinessRuleViolationException("Agency name is required.");

        return new CounselingReferralMade(
            Id.Value, Guid.NewGuid(), agencyName, referralDate,
            referredBy.Value);
    }

    /// <summary>Records the outcome of a counseling referral.</summary>
    public CounselingOutcomeRecorded RecordCounselingOutcome(
        Guid referralId, string outcome, DateOnly outcomeDate, UserId recordedBy)
    {
        GuardActive();
        if (string.IsNullOrWhiteSpace(outcome))
            throw new BusinessRuleViolationException("Outcome is required.");
        var referral = FindReferral(referralId);
        _ = referral;

        return new CounselingOutcomeRecorded(
            Id.Value, referralId, outcome, outcomeDate,
            recordedBy.Value);
    }

    /// <summary>Submits a forbearance request to a lender, advancing the case to Active if still Opened.</summary>
    public ForbearanceRequested RequestForbearance(
        string lenderName, DateOnly requestedDate, UserId requestedBy)
    {
        GuardNotResolved();
        if (string.IsNullOrWhiteSpace(lenderName))
            throw new BusinessRuleViolationException("Lender name is required.");

        return new ForbearanceRequested(
            Id.Value, Guid.NewGuid(), lenderName, requestedDate,
            requestedBy.Value);
    }

    /// <summary>Records the lender's decision on a forbearance request.</summary>
    public ForbearanceOutcomeRecorded RecordForbearanceOutcome(
        Guid requestId, ForbearanceOutcome outcome, DateOnly outcomeDate,
        string? notes, UserId recordedBy)
    {
        GuardActive();
        var request = FindForbearanceRequest(requestId);
        _ = request;

        return new ForbearanceOutcomeRecorded(
            Id.Value, requestId, outcome, outcomeDate, notes,
            recordedBy.Value);
    }

    /// <summary>Activates a CLT cure period under a lender recognition agreement, advancing the case to Active if still Opened.</summary>
    public CurePeriodActivated ActivateCurePeriod(
        string lraId, DateOnly activationDate, int curePeriodDays, UserId activatedBy)
    {
        GuardNotResolved();
        if (string.IsNullOrWhiteSpace(lraId))
            throw new BusinessRuleViolationException("LraId is required.");
        if (curePeriodDays <= 0)
            throw new BusinessRuleViolationException("Cure period days must be positive.");

        var cureDeadline = activationDate.AddDays(curePeriodDays);
        return new CurePeriodActivated(
            Id.Value, Guid.NewGuid(), lraId, activationDate, curePeriodDays, cureDeadline,
            activatedBy.Value);
    }

    /// <summary>Resolves the case with the given outcome.</summary>
    public ForeclosurePreventionCaseResolved Resolve(ForeclosurePreventionOutcome outcome, DateOnly resolvedDate, UserId resolvedBy)
    {
        if (Status != ForeclosurePreventionCaseStatus.Active)
            throw new BusinessRuleViolationException(
                "ForeclosurePreventionCase must be Active to resolve.");

        return new ForeclosurePreventionCaseResolved(
            Id.Value, outcome, resolvedDate,
            resolvedBy.Value);
    }

    /// <summary>Starts a foreclosure prevention case from its first event.</summary>
    public static ForeclosurePreventionCase Create(ForeclosurePreventionCaseOpened e)
    {
        ArgumentNullException.ThrowIfNull(e);

        var foreclosurePreventionCase = new ForeclosurePreventionCase();
        foreclosurePreventionCase.Apply(e);
        return foreclosurePreventionCase;
    }

    private void Apply(ForeclosurePreventionCaseOpened e)
    {
        Id = ForeclosurePreventionCaseId.Create(e.AggregateId);
        GroundLeaseId = e.GroundLeaseId;
        Trigger = e.Trigger;
        OpenedDate = e.OpenedDate;
        Status = ForeclosurePreventionCaseStatus.Opened;
    }

    /// <summary>Applies <see cref="CounselingReferralMade"/>.</summary>
    public void Apply(CounselingReferralMade e)
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

    /// <summary>Applies <see cref="CounselingOutcomeRecorded"/>.</summary>
    public void Apply(CounselingOutcomeRecorded e)
    {
        var referral = _counselingReferrals.First(r => r.Id == e.ReferralId);
        referral.Outcome = e.Outcome;
        referral.OutcomeDate = e.OutcomeDate;
    }

    /// <summary>Applies <see cref="ForbearanceRequested"/>.</summary>
    public void Apply(ForbearanceRequested e)
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

    /// <summary>Applies <see cref="ForbearanceOutcomeRecorded"/>.</summary>
    public void Apply(ForbearanceOutcomeRecorded e)
    {
        var request = _forbearanceRequests.First(r => r.Id == e.RequestId);
        request.Outcome = e.Outcome;
        request.OutcomeDate = e.OutcomeDate;
        request.Notes = e.Notes;
    }

    /// <summary>Applies <see cref="CurePeriodActivated"/>.</summary>
    public void Apply(CurePeriodActivated e)
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

    /// <summary>Applies <see cref="ForeclosurePreventionCaseResolved"/>.</summary>
    public void Apply(ForeclosurePreventionCaseResolved e)
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
