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
/// for a specific home. Its declaration starts as the household at submission and follows the household's
/// changes while the application is open (<see cref="FollowHousehold"/>); once selected it keeps the figures it
/// was chosen on.
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

    /// <summary>The day the application was approved; <see langword="null"/> before approval.</summary>
    public DateOnly? ApprovedDate { get; private set; }

    /// <summary>
    /// Set while an approved application's income or size differs from the figures staff last checked, until staff
    /// confirm the household still qualifies; <see langword="null"/> otherwise. A marked application can't be selected.
    /// </summary>
    public QualificationToConfirmMark? QualificationToConfirm { get; private set; }

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
    /// Takes the household's current figures. A submitted application just takes them. An approved one is also
    /// marked for staff to confirm it still qualifies when its income or size now differs from the figures staff
    /// last checked; a mark already open stays open from its first change, and clears if the figures go back to the
    /// checked ones. A change to the accessibility need alone marks nothing. A selected application keeps the figures
    /// it was chosen on, and a closed one keeps its last.
    /// </summary>
    /// <param name="current">The household's declaration now.</param>
    /// <param name="changedDate">Today in the trust's time zone.</param>
    /// <returns>The update, or <see langword="null"/> when nothing changed or the application no longer follows the household.</returns>
    public ApplicationDeclarationUpdated? FollowHousehold(Declaration current, DateOnly changedDate)
    {
        ArgumentNullException.ThrowIfNull(current);

        if (Phase != ApplicationPhase.Submitted && Phase != ApplicationPhase.Approved)
            return null;

        if (current.Equals(Declaration))
            return null;

        var mark = Phase == ApplicationPhase.Approved ? MarkAfter(current, changedDate) : null;

        return new ApplicationDeclarationUpdated(
            Id.Value,
            HouseholdId.Value,
            current.HouseholdSize,
            current.Circumstances.AnnualIncome,
            current.Circumstances.RequiresAccessibility,
            changedDate,
            mark);
    }

    /// <summary>
    /// Records that staff checked the changed figures and the household still qualifies, clearing the mark. The
    /// application stays approved and keeps its place on the waiting list. Staff whose check finds the household no
    /// longer qualifies remove it from the waiting list instead.
    /// </summary>
    /// <param name="confirmedDate">The day of the check; not after <paramref name="today"/> or before the mark's first change.</param>
    /// <param name="today">Today in the trust's time zone.</param>
    /// <param name="confirmedBy">The staff user confirming.</param>
    public ApplicationQualificationConfirmed ConfirmQualification(DateOnly confirmedDate, DateOnly today, UserId confirmedBy)
    {
        GuardNotTerminal();

        if (Phase != ApplicationPhase.Approved)
            throw BusinessRuleViolation.HomeownerApplication.ConfirmQualification.PhaseNotApproved(Phase);

        if (QualificationToConfirm is not { } mark)
            throw BusinessRuleViolation.HomeownerApplication.ConfirmQualification.NothingToConfirm();

        if (confirmedDate > today)
            throw BusinessRuleViolation.HomeownerApplication.ConfirmQualification.ConfirmedDateInFuture(confirmedDate, today);

        if (confirmedDate < mark.Since)
            throw BusinessRuleViolation.HomeownerApplication.ConfirmQualification.ConfirmedDateBeforeChange(confirmedDate, mark.Since);

        return new ApplicationQualificationConfirmed(
            Id.Value,
            HouseholdId.Value,
            confirmedDate,
            Declaration.Circumstances.AnnualIncome,
            Declaration.HouseholdSize,
            confirmedBy.Value);
    }

    private QualificationToConfirmMark? MarkAfter(Declaration current, DateOnly changedDate)
    {
        // The figures staff last checked: those the open mark holds, or else the declaration as it stands.
        var qualifiedIncome = QualificationToConfirm?.QualifiedAnnualIncome ?? Declaration.Circumstances.AnnualIncome;
        var qualifiedSize = QualificationToConfirm?.QualifiedHouseholdSize ?? Declaration.HouseholdSize;

        if (current.Circumstances.AnnualIncome == qualifiedIncome && current.HouseholdSize == qualifiedSize)
            return null;

        return new QualificationToConfirmMark(
            QualificationToConfirm?.Since ?? changedDate,
            qualifiedIncome,
            qualifiedSize);
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
    /// <param name="incomeLimitPercentOfAmi">The home's limit the selection judged by, or <see langword="null"/> when the home had none.</param>
    /// <param name="incomeLimits">The schedule in effect on <paramref name="selectedDate"/>, or <see langword="null"/> when none was.</param>
    public ApplicationSelectedForHome SelectForHome(
        Household household,
        HomeSelectionId selectionId,
        string homeId, string landParcelId, string groundLeaseTemplateId,
        decimal purchasePrice, DateOnly selectedDate, UserId selectedBy,
        int? incomeLimitPercentOfAmi,
        IncomeLimitTable? incomeLimits)
    {
        ArgumentNullException.ThrowIfNull(household);
        if (household.Id != HouseholdId)
            throw new ArgumentException("Household does not belong to this application.", nameof(household));
        GuardNotTerminal();

        if (Phase != ApplicationPhase.Approved)
            throw BusinessRuleViolation.HomeownerApplication.SelectApplicant.PhaseNotApproved(Phase);

        if (QualificationToConfirm is { } mark)
            throw BusinessRuleViolation.HomeownerApplication.SelectApplicant.QualificationToConfirm(mark.Since);

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
            Declaration.Circumstances.AnnualIncome,
            Declaration.HouseholdSize,
            incomeLimitPercentOfAmi,
            incomeLimits?.EffectiveFrom,
            incomeLimits?.ToEntries());
    }

    /// <summary>
    /// Records that another household was chosen for <paramref name="homeId"/> in <paramref name="selectionId"/> while
    /// this one could have been: the selection marked it passed over.
    /// </summary>
    /// <returns>The passing over, or <see langword="null"/> when it was already recorded for <paramref name="selectionId"/>.</returns>
    public ApplicationPassedOver? PassOver(HomeSelectionId selectionId, string homeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);

        if (_passedOverIn.Contains(selectionId))
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

    /// <summary>Denies an application under review. An approved household leaves through <see cref="RemoveFromWaitingList"/>.</summary>
    public ApplicationDenied DenyApplication(string reason, DateOnly deniedDate, UserId recordedBy)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw BusinessRuleViolation.HomeownerApplication.Deny.ReasonRequired();
        GuardNotTerminal();

        if (Phase != ApplicationPhase.Submitted)
            throw BusinessRuleViolation.HomeownerApplication.Deny.PhaseNotSubmitted(Phase);

        return new ApplicationDenied(Id.Value, reason, deniedDate, recordedBy.Value);
    }

    /// <summary>
    /// Takes an approved application off the waiting list for a reason that arose after approval.
    /// </summary>
    /// <param name="reason">Why.</param>
    /// <param name="note">Staff's note; required when <paramref name="reason"/> is Other.</param>
    /// <param name="removedDate">The day of removal; not after <paramref name="today"/> or before the approved date.</param>
    /// <param name="today">Today in the trust's time zone.</param>
    /// <param name="recordedBy">The staff user recording it.</param>
    public ApplicationRemovedFromWaitingList RemoveFromWaitingList(
        WaitingListRemovalReason reason, string? note, DateOnly removedDate, DateOnly today, UserId recordedBy)
    {
        ArgumentNullException.ThrowIfNull(reason);
        GuardNotTerminal();

        if (Phase != ApplicationPhase.Approved)
            throw BusinessRuleViolation.HomeownerApplication.RemoveFromWaitingList.PhaseNotApproved(Phase);

        var trimmedNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        if (reason == WaitingListRemovalReason.Other && trimmedNote is null)
            throw BusinessRuleViolation.HomeownerApplication.RemoveFromWaitingList.NoteRequired();

        if (removedDate > today)
            throw BusinessRuleViolation.HomeownerApplication.RemoveFromWaitingList.RemovedDateInFuture(removedDate, today);

        if (removedDate < ApprovedDate)
            throw BusinessRuleViolation.HomeownerApplication.RemoveFromWaitingList.RemovedDateBeforeApproval(removedDate, ApprovedDate.Value);

        return new ApplicationRemovedFromWaitingList(Id.Value, reason, trimmedNote, removedDate, recordedBy.Value);
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
        ApprovedDate = e.ApprovedDate;
    }
    
    /// <summary>Applies <see cref="ApplicationDeclarationUpdated"/>.</summary>
    public void Apply(ApplicationDeclarationUpdated e)
    {
        Declaration = new Declaration(e.HouseholdSize, HouseholdCircumstances.Create(e.AccessibilityRequired, e.AnnualIncome));
        QualificationToConfirm = e.QualificationToConfirm;
    }

    /// <summary>Applies <see cref="ApplicationQualificationConfirmed"/>.</summary>
    public void Apply(ApplicationQualificationConfirmed _)
    {
        QualificationToConfirm = null;
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

    /// <summary>Applies <see cref="ApplicationRemovedFromWaitingList"/>.</summary>
    public void Apply(ApplicationRemovedFromWaitingList _)
    {
        Phase = ApplicationPhase.Removed;
    }

    private void GuardNotTerminal()
    {
        if (IsTerminal)
            throw BusinessRuleViolation.HomeownerApplication.Modify.TerminalPhase(Phase);

        // A selected application belongs to a purchase in progress. It leaves Selected only through that
        // purchase: back to the waiting list if it falls through, or Purchased when it closes.
        if (Phase == ApplicationPhase.Selected)
            throw BusinessRuleViolation.HomeownerApplication.Modify.SelectedForPurchase();
    }
}