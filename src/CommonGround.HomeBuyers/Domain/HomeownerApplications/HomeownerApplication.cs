using CommonGround.HomeBuyers.Application.Errors;
using CommonGround.HomeBuyers.Domain.HomeownerApplications.Events;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;
using Marten.Linq.CreatedAt;

namespace CommonGround.HomeBuyers.Domain.HomeownerApplications;

/// <summary>
/// A household's application to buy a CLT home, from submission through approval to selection
/// for a specific home. Captures a snapshot of the household at submission.
/// </summary>
public sealed class HomeownerApplication : AggregateRoot<HomeownerApplicationId>
{
#pragma warning disable CS8618
    private HomeownerApplication() { }
#pragma warning restore CS8618

    public HouseholdId HouseholdId { get; private set; }
    public Declaration Declaration { get; private set; }
    public ApplicationPhase Phase { get; private set; }
    public DateTimeOffset SubmittedAt { get; private set; }
    public UserId SubmittedBy { get; private set; }
    public string? DenialReason { get; private set; }
    public string? WithdrawalReason { get; private set; }

    public bool IsTerminal => Phase.IsTerminal;

    public static HomeownerApplication Submit(
        HouseholdId householdId,
        Declaration declaration,
        UserId submittedBy)
    {
        ArgumentNullException.ThrowIfNull(householdId);
        ArgumentNullException.ThrowIfNull(declaration);
        ArgumentNullException.ThrowIfNull(submittedBy);

        var application = new HomeownerApplication();
        application.ApplyChange(new ApplicationSubmitted(
            HomeownerApplicationId.Generate().Value,
            householdId.Value,
            declaration.HouseholdSize,
            declaration.Circumstances.AnnualIncome,
            declaration.Circumstances.RequiresAccessibility,
            submittedBy.Value));

        return application;
    }

    public void Approve(DateOnly approvedDate, string? notes, UserId recordedBy)
    {
        ArgumentNullException.ThrowIfNull(recordedBy);
        GuardNotTerminal();

        if (Phase != ApplicationPhase.Submitted)
            throw BusinessRuleViolation.HomeownerApplication.Approve.PhaseNotSubmitted(Phase);

        ApplyChange(new ApplicationApproved(Id.Value, approvedDate, notes, recordedBy.Value));
    }

    public void SelectForHome(
        string applicantFullName, string applicantEmail, string? applicantPhone,
        string homeId, string landParcelId, string groundLeaseTemplateId,
        decimal purchasePrice, DateOnly selectedDate, UserId selectedBy)
    {
        ArgumentNullException.ThrowIfNull(selectedBy);
        ArgumentException.ThrowIfNullOrWhiteSpace(applicantFullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(applicantEmail);
        GuardNotTerminal();

        if (Phase != ApplicationPhase.Approved)
            throw BusinessRuleViolation.HomeownerApplication.SelectApplicant.PhaseNotApproved(Phase);

        if (purchasePrice <= 0)
            throw BusinessRuleViolation.HomeownerApplication.SelectApplicant.PurchasePriceNonPositive(purchasePrice);

        ApplyChange(new ApplicationSelectedForHome(
            Id.Value,
            HouseholdId.Value,
            applicantFullName,
            applicantEmail,
            applicantPhone,
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

    public void Withdraw(DateOnly withdrawnDate, string? reason, UserId recordedBy)
    {
        GuardNotTerminal();
        ApplyChange(new ApplicationWithdrawn(Id.Value, withdrawnDate, reason, recordedBy.Value));
    }

    protected override void ApplyEvent(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case ApplicationSubmitted e: Apply(e); break;
            case ApplicationApproved e: Apply(e); break;
            case ApplicationSelectedForHome e: Apply(e); break;
            case ApplicationDenied e: Apply(e); break;
            case ApplicationWithdrawn e: Apply(e); break;
            default: throw new InvalidOperationException(
                $"Unknown event: {domainEvent?.GetType().Name}");
        }
    }

    private void Apply(ApplicationSubmitted e)
    {
        Id = HomeownerApplicationId.Create(e.AggregateId);
        HouseholdId = HouseholdId.Create(e.HouseholdId);
        Declaration = new Declaration(e.HouseholdSize, HouseholdCircumstances.Create(e.AccessibilityRequired, e.AnnualIncome));
        Phase = ApplicationPhase.Submitted;
        SubmittedAt = e.OccurredAt;
        SubmittedBy = UserId.Create(e.SubmittedBy);
    }

    private void Apply(ApplicationSelectedForHome _)
    {
        Phase = ApplicationPhase.Selected;
    }

    private void Apply(ApplicationApproved e)
    {
        Phase = ApplicationPhase.Approved;
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

    private void GuardNotTerminal()
    {
        if (IsTerminal)
            throw BusinessRuleViolation.HomeownerApplication.Modify.TerminalPhase(Phase);
    }
}