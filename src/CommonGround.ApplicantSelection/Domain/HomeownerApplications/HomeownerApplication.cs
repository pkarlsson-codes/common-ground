using CommonGround.ApplicantSelection.Application.Errors;
using CommonGround.ApplicantSelection.Domain.HomeownerApplications.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.ApplicantSelection.Domain.HomeownerApplications;

/// <summary>
/// Tracks a prospective homeowner from initial application through selection for a home.
/// Parallel checklist: housing counseling, CLT orientation, and mortgage prequalification
/// must all be satisfied before an applicant can be added to the waiting list.
/// </summary>
public sealed class HomeownerApplication : AggregateRoot<HomeownerApplicationId>
{
#pragma warning disable CS8618
    private HomeownerApplication() { }
#pragma warning restore CS8618

    public Applicant Applicant { get; private set; }
    public HouseholdInfo HouseholdInfo { get; private set; }
    public ApplicationPhase Phase { get; private set; }
    public DateTimeOffset SubmittedAt { get; private set; }
    public UserId SubmittedBy { get; private set; }

    public DateOnly? EligibilityApprovedDate { get; private set; }
    public bool IsHousingCounselingCompleted { get; private set; }
    public bool IsCltOrientationCompleted { get; private set; }
    public MortgagePrequalification? MortgagePrequalification { get; private set; }

    public string? DenialReason { get; private set; }
    public string? WithdrawalReason { get; private set; }

    public bool IsTerminal => Phase.IsTerminal;

    public bool IsPrequalificationExpired =>
        MortgagePrequalification is not null &&
        MortgagePrequalification.ExpiryDate < DateOnly.FromDateTime(DateTime.UtcNow);

    private bool AllChecklistItemsSatisfied =>
        IsHousingCounselingCompleted &&
        IsCltOrientationCompleted &&
        MortgagePrequalification is not null;

    public static HomeownerApplication Submit(
        Applicant applicant,
        HouseholdInfo householdInfo,
        UserId submittedBy)
    {
        ArgumentNullException.ThrowIfNull(applicant);
        ArgumentNullException.ThrowIfNull(householdInfo);
        ArgumentNullException.ThrowIfNull(submittedBy);

        var application = new HomeownerApplication();
        application.ApplyChange(new ApplicationSubmitted(
            HomeownerApplicationId.Generate().Value,
            applicant.FullName,
            applicant.Email,
            applicant.Phone?.Value,
            householdInfo.HouseholdSize,
            householdInfo.AnnualIncome,
            householdInfo.AccessibilityRequired,
            submittedBy.Value));

        return application;
    }

    public void ApproveEligibility(DateOnly approvedDate, string? notes, UserId recordedBy)
    {
        ArgumentNullException.ThrowIfNull(recordedBy);
        GuardNotTerminal();

        if (Phase != ApplicationPhase.Submitted)
            throw BusinessRuleViolation.HomeownerApplication.ApproveEligibility.PhaseNotSubmitted(Phase);

        ApplyChange(new EligibilityApproved(Id.Value, approvedDate, notes, recordedBy.Value));
        AdvanceToWaitlistedIfReady();
    }

    public void DenyEligibility(string reason, DateOnly deniedDate, UserId recordedBy)
    {
        ArgumentNullException.ThrowIfNull(recordedBy);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        GuardNotTerminal();

        if (Phase != ApplicationPhase.Submitted)
            throw BusinessRuleViolation.HomeownerApplication.DenyEligibility.PhaseNotSubmitted(Phase);

        ApplyChange(new EligibilityDenied(Id.Value, reason, deniedDate, recordedBy.Value));
    }

    /// <summary>Records HUD-approved housing counseling completion. Idempotent.</summary>
    public void RecordHousingCounseling(
        string certificationNumber, DateOnly completedDate,
        string counselorOrganization, UserId recordedBy)
    {
        ArgumentNullException.ThrowIfNull(recordedBy);
        GuardNotTerminal();
        if (IsHousingCounselingCompleted) return;

        ApplyChange(new HousingCounselingRecorded(
            Id.Value, certificationNumber, completedDate, counselorOrganization, recordedBy.Value));
        AdvanceToWaitlistedIfReady();
    }

    /// <summary>Records CLT orientation completion. Idempotent.</summary>
    public void RecordCltOrientation(DateOnly completedDate, UserId recordedBy)
    {
        ArgumentNullException.ThrowIfNull(recordedBy);
        GuardNotTerminal();
        if (IsCltOrientationCompleted) return;

        ApplyChange(new CltOrientationRecorded(Id.Value, completedDate, recordedBy.Value));
        AdvanceToWaitlistedIfReady();
    }

    /// <summary>Records a mortgage prequalification. Replaces any existing prequalification.</summary>
    public void RecordMortgagePrequalification(
        string lenderName, decimal approvedAmount,
        DateOnly expiryDate, UserId recordedBy)
    {
        ArgumentNullException.ThrowIfNull(recordedBy);
        GuardNotTerminal();

        ApplyChange(new MortgagePrequalificationRecorded(
            Id.Value, lenderName, approvedAmount, expiryDate, recordedBy.Value));
        AdvanceToWaitlistedIfReady();
    }

    /// <summary>
    /// Marks the prequalification as expired. Only applies if the recorded expiry date matches,
    /// guarding against stale scheduled messages firing after a renewal.
    /// Returns true if an event was applied.
    /// </summary>
    public bool ExpirePrequalification(DateOnly originalExpiryDate)
    {
        if (IsTerminal) return false;
        if (MortgagePrequalification?.ExpiryDate != originalExpiryDate) return false;

        ApplyChange(new MortgagePrequalificationExpired(Id.Value, originalExpiryDate));
        return true;
    }

    public void SelectForHome(
        string homeId, string landParcelId, string groundLeaseTemplateId,
        decimal purchasePrice, DateOnly selectedDate, UserId selectedBy)
    {
        ArgumentNullException.ThrowIfNull(selectedBy);
        GuardNotTerminal();

        if (Phase != ApplicationPhase.Waitlisted)
            throw BusinessRuleViolation.HomeownerApplication.SelectApplicant.PhaseNotWaitlisted(Phase);

        if (MortgagePrequalification is null || MortgagePrequalification.ExpiryDate < selectedDate)
            throw BusinessRuleViolation.HomeownerApplication.SelectApplicant.PrequalificationExpired();

        if (purchasePrice <= 0)
            throw BusinessRuleViolation.HomeownerApplication.SelectApplicant.PurchasePriceNonPositive(purchasePrice);

        ApplyChange(new ApplicantSelectedForHome(
            Id.Value,
            Applicant.FullName,
            Applicant.Email,
            Applicant.Phone?.Value,
            homeId,
            landParcelId,
            groundLeaseTemplateId,
            purchasePrice,
            selectedDate,
            selectedBy.Value));
    }

    public void DenyApplication(string reason, DateOnly deniedDate, UserId recordedBy)
    {
        ArgumentNullException.ThrowIfNull(recordedBy);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        GuardNotTerminal();

        ApplyChange(new ApplicationDenied(Id.Value, reason, deniedDate, recordedBy.Value));
    }

    public void Withdraw(DateOnly withdrawnDate, string? reason)
    {
        GuardNotTerminal();
        ApplyChange(new ApplicationWithdrawn(Id.Value, withdrawnDate, reason));
    }

    protected override void ApplyEvent(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case ApplicationSubmitted e: Apply(e); break;
            case EligibilityApproved e: Apply(e); break;
            case EligibilityDenied e: Apply(e); break;
            case HousingCounselingRecorded e: Apply(e); break;
            case CltOrientationRecorded e: Apply(e); break;
            case MortgagePrequalificationRecorded e: Apply(e); break;
            case MortgagePrequalificationExpired e: Apply(e); break;
            case AddedToWaitingList e: Apply(e); break;
            case ApplicantSelectedForHome e: Apply(e); break;
            case ApplicationDenied e: Apply(e); break;
            case ApplicationWithdrawn e: Apply(e); break;
            default: throw new InvalidOperationException(
                $"Unknown event: {domainEvent?.GetType().Name}");
        }
    }

    private void Apply(ApplicationSubmitted e)
    {
        Id = HomeownerApplicationId.Create(e.AggregateId);
        Applicant = new Applicant(e.ApplicantFullName, e.ApplicantEmail, PhoneNumber.FromOptional(e.ApplicantPhone));
        HouseholdInfo = new HouseholdInfo(e.HouseholdSize, e.AnnualIncome, e.AccessibilityRequired);
        Phase = ApplicationPhase.Submitted;
        SubmittedAt = e.OccurredAt;
        SubmittedBy = UserId.Create(e.SubmittedBy);
    }

    private void Apply(EligibilityApproved e)
    {
        EligibilityApprovedDate = e.ApprovedDate;
        Phase = ApplicationPhase.Active;
    }

    private void Apply(EligibilityDenied e)
    {
        DenialReason = e.Reason;
        Phase = ApplicationPhase.EligibilityDenied;
    }

    private void Apply(HousingCounselingRecorded _)
    {
        IsHousingCounselingCompleted = true;
    }

    private void Apply(CltOrientationRecorded _)
    {
        IsCltOrientationCompleted = true;
    }

    private void Apply(MortgagePrequalificationRecorded e)
    {
        MortgagePrequalification = new MortgagePrequalification(
            e.LenderName, e.ApprovedAmount, e.ExpiryDate);
    }

    private void Apply(MortgagePrequalificationExpired _)
    {
        MortgagePrequalification = null;
        if (Phase == ApplicationPhase.Waitlisted)
            Phase = ApplicationPhase.Active;
    }

    private void Apply(AddedToWaitingList _)
    {
        Phase = ApplicationPhase.Waitlisted;
    }

    private void Apply(ApplicantSelectedForHome _)
    {
        Phase = ApplicationPhase.Selected;
    }

    private void Apply(ApplicationDenied e)
    {
        DenialReason = e.Reason;
        Phase = ApplicationPhase.Denied;
    }

    private void Apply(ApplicationWithdrawn e)
    {
        WithdrawalReason = e.Reason;
        Phase = ApplicationPhase.Withdrawn;
    }

    private void AdvanceToWaitlistedIfReady()
    {
        if (Phase == ApplicationPhase.Active && AllChecklistItemsSatisfied)
            ApplyChange(new AddedToWaitingList(Id.Value, DateOnly.FromDateTime(DateTime.UtcNow)));
    }

    private void GuardNotTerminal()
    {
        if (IsTerminal)
            throw BusinessRuleViolation.HomeownerApplication.Modify.TerminalPhase(Phase);
    }
}