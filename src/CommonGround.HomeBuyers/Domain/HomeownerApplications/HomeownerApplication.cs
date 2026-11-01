using CommonGround.HomeBuyers.Application.Errors;
using CommonGround.HomeBuyers.Domain.HomeownerApplications.Events;
using CommonGround.HomeBuyers.Domain.HomeSelections;
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
    /// <summary>Interim: Marten 9 only calls public constructors. Removed once this aggregate is a decider.</summary>
    public HomeownerApplication() { }
#pragma warning restore CS8618

    public HouseholdId HouseholdId { get; private set; }
    public Declaration Declaration { get; private set; }
    public ApplicationPhase Phase { get; private set; }
    public DateTimeOffset SubmittedAt { get; private set; }
    public UserId SubmittedBy { get; private set; }
    public string? DenialReason { get; private set; }
    public string? WithdrawalReason { get; private set; }

    public bool IsTerminal => Phase.IsTerminal;

    /// <summary>
    /// Submits a new application for a household. Rejected when <paramref name="openApplicationId"/>
    /// (the household's existing non-terminal application, if any) is set.
    /// </summary>
    public static HomeownerApplication Submit(
        HouseholdId householdId,
        Declaration declaration,
        HomeownerApplicationId? openApplicationId,
        UserId submittedBy)
    {
        ArgumentNullException.ThrowIfNull(declaration);

        if (openApplicationId is not null)
            throw BusinessRuleViolation.HomeownerApplication.Submit.HouseholdHasOpenApplication(openApplicationId.Value);

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
        GuardNotTerminal();

        if (Phase != ApplicationPhase.Submitted)
            throw BusinessRuleViolation.HomeownerApplication.Approve.PhaseNotSubmitted(Phase);

        ApplyChange(new ApplicationApproved(Id.Value, approvedDate, notes, recordedBy.Value));
    }

    /// <summary>
    /// Whether <see cref="SelectForHome"/> would accept this application with
    /// <paramref name="household"/> on <paramref name="selectedDate"/>.
    /// </summary>
    public bool CanBeSelected(Household household, DateOnly selectedDate)
    {
        ArgumentNullException.ThrowIfNull(household);

        return household.Id == HouseholdId
            && Phase == ApplicationPhase.Approved
            && household.PrimaryMember is not null
            && household.IsReadinessComplete
            && household.IsPrequalificationValidOn(selectedDate);
    }

    /// <summary>
    /// Selects the application for a home. The household must be this application's household,
    /// have completed readiness, and hold a prequalification still valid on <paramref name="selectedDate"/>.
    /// </summary>
    public void SelectForHome(
        Household household,
        string homeId, string landParcelId, string groundLeaseTemplateId,
        decimal purchasePrice, DateOnly selectedDate, UserId selectedBy)
    {
        ArgumentNullException.ThrowIfNull(household);
        if (household.Id != HouseholdId)
            throw new ArgumentException("Household does not belong to this application.", nameof(household));
        GuardNotTerminal();

        if (Phase != ApplicationPhase.Approved)
            throw BusinessRuleViolation.HomeownerApplication.SelectApplicant.PhaseNotApproved(Phase);

        if (purchasePrice <= 0)
            throw BusinessRuleViolation.HomeownerApplication.SelectApplicant.PurchasePriceNonPositive(purchasePrice);

        var applicant = household.PrimaryMember
            ?? throw BusinessRuleViolation.Household.SelectApplicant.NoPrimaryMember(household.Id);

        if (!household.IsReadinessComplete)
            throw BusinessRuleViolation.HomeownerApplication.SelectApplicant.HouseholdReadinessIncomplete(
                household.OutstandingReadinessItems);

        if (!household.IsPrequalificationValidOn(selectedDate))
            throw BusinessRuleViolation.HomeownerApplication.SelectApplicant.PrequalificationExpired(
                household.MortgagePrequalification!.ExpiryDate, selectedDate);

        ApplyChange(new ApplicationSelectedForHome(
            Id.Value,
            HouseholdId.Value,
            applicant.Name,
            applicant.Email,
            applicant.Phone,
            homeId,
            landParcelId,
            groundLeaseTemplateId,
            purchasePrice,
            selectedDate,
            selectedBy.Value));
    }

    /// <summary>Records that another household was chosen for <paramref name="homeId"/> in <paramref name="selectionId"/>.</summary>
    public void PassOver(HomeSelectionId selectionId, string homeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);

        if (Phase != ApplicationPhase.Approved)
            throw BusinessRuleViolation.HomeownerApplication.PassOver.PhaseNotApproved(Phase);

        ApplyChange(new ApplicationPassedOver(Id.Value, HouseholdId.Value, selectionId.Value, homeId));
    }

    /// <summary>
    /// Returns the application to the waiting list after the purchase of <paramref name="homeId"/> it was
    /// selected for fell through. It keeps its original approval date, so the household keeps its place.
    /// </summary>
    public void ReturnToWaitingList(string homeId, string reason, DateOnly returnedDate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        if (Phase != ApplicationPhase.Selected)
            throw BusinessRuleViolation.HomeownerApplication.ReturnToWaitingList.PhaseNotSelected(Phase);

        ApplyChange(new ApplicationReturnedToWaitingList(Id.Value, HouseholdId.Value, homeId, reason, returnedDate));
    }

    /// <summary>Records that the purchase of <paramref name="homeId"/> the application was selected for has closed.</summary>
    public void RecordPurchaseClosed(string homeId, DateOnly closingDate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);

        if (Phase != ApplicationPhase.Selected)
            throw BusinessRuleViolation.HomeownerApplication.RecordPurchaseClosed.PhaseNotSelected(Phase);

        ApplyChange(new ApplicationPurchaseClosed(Id.Value, HouseholdId.Value, homeId, closingDate));
    }

    public void DenyApplication(string reason, DateOnly deniedDate, UserId recordedBy)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw BusinessRuleViolation.HomeownerApplication.Deny.ReasonRequired();
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
            case ApplicationPassedOver: break;
            case ApplicationReturnedToWaitingList e: Apply(e); break;
            case ApplicationPurchaseClosed e: Apply(e); break;
            case ApplicationDenied e: Apply(e); break;
            case ApplicationWithdrawn e: Apply(e); break;
            default: throw new InvalidOperationException(
                $"Unknown event: {domainEvent?.GetType().Name}");
        }
    }

    /// <summary>Applies <see cref="ApplicationSubmitted"/>.</summary>

    public void Apply(ApplicationSubmitted e)
    {
        Id = HomeownerApplicationId.Create(e.AggregateId);
        HouseholdId = HouseholdId.Create(e.HouseholdId);
        Declaration = new Declaration(e.HouseholdSize, HouseholdCircumstances.Create(e.AccessibilityRequired, e.AnnualIncome));
        Phase = ApplicationPhase.Submitted;
        SubmittedAt = e.OccurredAt;
        SubmittedBy = UserId.Create(e.SubmittedBy);
    }

    /// <summary>Applies <see cref="ApplicationSelectedForHome"/>.</summary>

    public void Apply(ApplicationSelectedForHome _)
    {
        Phase = ApplicationPhase.Selected;
    }

    /// <summary>Applies <see cref="ApplicationApproved"/>.</summary>

    public void Apply(ApplicationApproved e)
    {
        Phase = ApplicationPhase.Approved;
    }
    
    /// <summary>Applies <see cref="ApplicationReturnedToWaitingList"/>.</summary>
    
    public void Apply(ApplicationReturnedToWaitingList _)
    {
        Phase = ApplicationPhase.Approved;
    }

    /// <summary>Applies <see cref="ApplicationPurchaseClosed"/>.</summary>

    public void Apply(ApplicationPurchaseClosed _)
    {
        Phase = ApplicationPhase.Purchased;
    }

    /// <summary>Applies <see cref="ApplicationDenied"/>.</summary>

    public void Apply(ApplicationDenied e)
    {
        DenialReason = e.Reason;
        Phase = ApplicationPhase.Denied;
    }

    /// <summary>Applies <see cref="ApplicationWithdrawn"/>.</summary>

    public void Apply(ApplicationWithdrawn e)
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