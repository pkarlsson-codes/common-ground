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
#pragma warning disable CS8618 // State is set by Create from the stream's first event.
    private HomeownerApplication() { }
#pragma warning restore CS8618

    public HouseholdId HouseholdId { get; private set; }
    public Declaration Declaration { get; private set; }
    public ApplicationPhase Phase { get; private set; }
    public DateTimeOffset SubmittedAt { get; private set; }
    public UserId SubmittedBy { get; private set; }
    public string? DenialReason { get; private set; }
    public string? WithdrawalReason { get; private set; }

    private readonly HashSet<HomeSelectionId> _passedOverIn = [];

    /// <summary>The home selections that completed with another household chosen while this application could have been.</summary>
    public IReadOnlySet<HomeSelectionId> PassedOverIn => _passedOverIn;

    public bool IsTerminal => Phase.IsTerminal;

    /// <summary>
    /// Submits a new application for a household. Rejected when <paramref name="openApplicationId"/>
    /// (the household's existing non-terminal application, if any) is set.
    /// </summary>
    public static ApplicationSubmitted Submit(
        HomeownerApplicationId id,
        HouseholdId householdId,
        Declaration declaration,
        HomeownerApplicationId? openApplicationId,
        UserId submittedBy)
    {
        ArgumentNullException.ThrowIfNull(declaration);

        if (openApplicationId is not null)
            throw BusinessRuleViolation.HomeownerApplication.Submit.HouseholdHasOpenApplication(openApplicationId.Value);

        return new ApplicationSubmitted(
            id.Value,
            householdId.Value,
            declaration.HouseholdSize,
            declaration.Circumstances.AnnualIncome,
            declaration.Circumstances.RequiresAccessibility,
            submittedBy.Value);
    }

    public ApplicationApproved Approve(DateOnly approvedDate, string? notes, UserId recordedBy)
    {
        GuardNotTerminal();

        if (Phase != ApplicationPhase.Submitted)
            throw BusinessRuleViolation.HomeownerApplication.Approve.PhaseNotSubmitted(Phase);

        return new ApplicationApproved(Id.Value, approvedDate, notes, recordedBy.Value);
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
    /// Selects the application for a home in <paramref name="selectionId"/>. The household must be this
    /// application's household, have completed readiness, and hold a prequalification still valid on
    /// <paramref name="selectedDate"/>.
    /// </summary>
    /// <param name="household">This application's household.</param>
    /// <param name="selectionId">The selection the application is chosen in.</param>
    /// <param name="homeId">The home.</param>
    /// <param name="landParcelId">The home's land parcel.</param>
    /// <param name="groundLeaseTemplateId">The ground lease template the purchase will use.</param>
    /// <param name="purchasePrice">The purchase price, above zero.</param>
    /// <param name="selectedDate">The date of the selection.</param>
    /// <param name="selectedBy">The staff user choosing.</param>
    /// <param name="candidate">The application's candidate in the selection: the income and size it was judged on.</param>
    /// <param name="incomeLimitPercentOfAmi">The home's limit the selection judged by, or <see langword="null"/> when the home had none.</param>
    /// <param name="incomeLimits">The schedule in effect when the selection started, or <see langword="null"/> when none was.</param>
    public ApplicationSelectedForHome SelectForHome(
        Household household,
        HomeSelectionId selectionId,
        string homeId, string landParcelId, string groundLeaseTemplateId,
        decimal purchasePrice, DateOnly selectedDate, UserId selectedBy,
        SelectionCandidate candidate,
        int? incomeLimitPercentOfAmi,
        IncomeLimitTable? incomeLimits)
    {
        ArgumentNullException.ThrowIfNull(household);
        ArgumentNullException.ThrowIfNull(candidate);
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

        return new ApplicationSelectedForHome(
            Id.Value,
            HouseholdId.Value,
            applicant.Name.Value,
            applicant.Email.Value,
            applicant.Phone.Value,
            selectionId.Value,
            homeId,
            landParcelId,
            groundLeaseTemplateId,
            purchasePrice,
            selectedDate,
            selectedBy.Value,
            candidate.AnnualIncome,
            candidate.HouseholdSize,
            incomeLimitPercentOfAmi,
            incomeLimits?.EffectiveFrom,
            incomeLimits?.ToEntries());
    }

    /// <summary>
    /// Records that another household was chosen for <paramref name="homeId"/> in <paramref name="selectionId"/>.
    /// An application that couldn't have been selected on <paramref name="selectedDate"/>, because it withdrew
    /// or its household's readiness lapsed, was never really in the running, so it isn't passed over.
    /// </summary>
    /// <returns>
    /// The passing over, or <see langword="null"/> when the application couldn't have been selected or was
    /// already passed over in <paramref name="selectionId"/>.
    /// </returns>
    public ApplicationPassedOver? PassOver(HomeSelectionId selectionId, string homeId, Household household, DateOnly selectedDate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);

        if (_passedOverIn.Contains(selectionId) || !CanBeSelected(household, selectedDate))
            return null;

        return new ApplicationPassedOver(Id.Value, HouseholdId.Value, selectionId.Value, homeId);
    }

    /// <summary>
    /// Returns the application to the waiting list after the purchase of <paramref name="homeId"/> it was
    /// selected for fell through. It keeps its original approval date, so the household keeps its place.
    /// </summary>
    /// <returns>The return, or <see langword="null"/> when the application is already back on the waiting list.</returns>
    public ApplicationReturnedToWaitingList? ReturnToWaitingList(string homeId, string reason, DateOnly returnedDate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        if (Phase == ApplicationPhase.Approved)
            return null;

        if (Phase != ApplicationPhase.Selected)
            throw BusinessRuleViolation.HomeownerApplication.ReturnToWaitingList.PhaseNotSelected(Phase);

        return new ApplicationReturnedToWaitingList(Id.Value, HouseholdId.Value, homeId, reason, returnedDate);
    }

    /// <summary>Records that the purchase of <paramref name="homeId"/> the application was selected for has closed.</summary>
    /// <returns>The closing, or <see langword="null"/> when it is already recorded.</returns>
    public ApplicationPurchaseClosed? RecordPurchaseClosed(string homeId, DateOnly closingDate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);

        if (Phase == ApplicationPhase.Purchased)
            return null;

        if (Phase != ApplicationPhase.Selected)
            throw BusinessRuleViolation.HomeownerApplication.RecordPurchaseClosed.PhaseNotSelected(Phase);

        return new ApplicationPurchaseClosed(Id.Value, HouseholdId.Value, homeId, closingDate);
    }

    public ApplicationDenied DenyApplication(string reason, DateOnly deniedDate, UserId recordedBy)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw BusinessRuleViolation.HomeownerApplication.Deny.ReasonRequired();
        GuardNotTerminal();

        return new ApplicationDenied(Id.Value, reason, deniedDate, recordedBy.Value);
    }

    public ApplicationWithdrawn Withdraw(DateOnly withdrawnDate, string? reason, UserId recordedBy)
    {
        GuardNotTerminal();
        return new ApplicationWithdrawn(Id.Value, withdrawnDate, reason, recordedBy.Value);
    }

    /// <summary>Starts a homeowner application from its first event.</summary>
    public static HomeownerApplication Create(ApplicationSubmitted e)
    {
        ArgumentNullException.ThrowIfNull(e);

        var homeownerApplication = new HomeownerApplication();
        homeownerApplication.Apply(e);
        return homeownerApplication;
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

    /// <summary>Applies <see cref="ApplicationPassedOver"/>.</summary>
    public void Apply(ApplicationPassedOver e)
    {
        _passedOverIn.Add(HomeSelectionId.Create(e.SelectionId));
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