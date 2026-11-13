using CommonGround.Homeownership.Domain.GroundLeases.Events;
using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.Homeownership.Domain.GroundLeases.Mortgages;
using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.Homeownership.Domain.GroundLeaseTemplates;
using CommonGround.Homeownership.Application.Errors;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;
using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.Homeownership.Domain.MortgageRequests;
using CommonGround.Homeownership.Domain.Resales;

namespace CommonGround.Homeownership.Domain.GroundLeases;

/// <summary>
/// A ground lease between the CLT and a homeowner.
/// The CLT retains land ownership; the homeowner leases the land and owns the structure.
/// </summary>
public sealed class GroundLease : AggregateRoot<GroundLeaseId>
{
#pragma warning disable CS8618 // State is set by Create from the stream's first event.
    private GroundLease() { }
#pragma warning restore CS8618

    /// <summary>Gets the ID of the land parcel this lease is for.</summary>
    public string LandParcelId { get; private set; }

    /// <summary>Gets the ID of the leasable unit on this leased parcel.</summary>
    public string UnitId { get; private set; }

    /// <summary>Gets the start date of the lease.</summary>
    public DateOnly LeaseStartDate { get; private set; }

    /// <summary>Gets the term of the lease in years.</summary>
    public int TermYears { get; private set; }

    /// <summary>
    /// Gets the days the trust has to buy the home, or assign its option, after the homeowner gives notice to
    /// sell. Copied from the template when the lease was created, so editing the template later doesn't change it.
    /// </summary>
    public int OptionWindowDays { get; private set; }

    /// <summary>Gets what the homeowner paid for the home, which the resale formula starts from.</summary>
    public decimal BasePrice { get; private set; }

    /// <summary>Gets the home's appraised value when the homeowner bought it, if known.</summary>
    public decimal? OriginalAppraisedValue { get; private set; }

    /// <summary>Gets the covenant terms for this lease.</summary>
    public string CovenantTerms { get; private set; }

    /// <summary>Gets the resale policy terms for this lease.</summary>
    public ResalePolicyTerms ResalePolicyTerms { get; private set; }

    /// <summary>Gets the current homeowner, if any.</summary>
    public Homeowner? CurrentHomeowner { get; private set; }

    /// <summary>Gets the status of this ground lease.</summary>
    public GroundLeaseStatus Status { get; private set; }

    /// <summary>Gets when this lease was created.</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Gets the user who created this lease.</summary>
    public UserId CreatedBy { get; private set; }

    /// <summary>
    /// Gets the source template ID when this lease was created from a template.
    /// Null indicates this lease was created directly without a template.
    /// </summary>
    public GroundLeaseTemplateId? TemplateId { get; private set; }

    /// <summary>Gets the source purchase workflow ID when this lease was created from a purchase workflow.</summary>
    public PurchaseWorkflowId? PurchaseWorkflowId { get; private set; }

    /// <summary>Gets the resale in progress on this lease, if its homeowner has given notice to sell.</summary>
    public ResaleId? OpenResaleId { get; private set; }

    /// <summary>
    /// Gets the monthly lease fee. Copied from the template when the lease was created, so editing the template
    /// later doesn't change it.
    /// </summary>
    public LeaseFeeTerms LeaseFeeTerms { get; private set; }

    /// <summary>
    /// Gets the most total mortgage debt may be when the trust permits a new loan (Art 8.3). Copied from the template
    /// when the lease was created, so editing the template later doesn't change it.
    /// </summary>
    public MortgageDebtCap MortgageDebtCap { get; private set; }

    /// <summary>Gets the day the lease's fees are tracked from and what was owed then.</summary>
    public LeaseFeeTracking LeaseFeeTracking { get; private set; }

    /// <summary>Gets the day the lease ended, or null while it is active.</summary>
    public DateOnly? EndedOn { get; private set; }

    private readonly List<LeaseFeePayment> _leaseFeePayments = [];

    /// <summary>Gets every lease fee payment recorded on this lease, voided ones included, in the order recorded.</summary>
    public IReadOnlyList<LeaseFeePayment> LeaseFeePayments => _leaseFeePayments;

    private readonly List<LeaseFeeNotice> _leaseFeeNotices = [];

    /// <summary>Gets every late notice recorded on this lease, in the order recorded.</summary>
    public IReadOnlyList<LeaseFeeNotice> LeaseFeeNotices => _leaseFeeNotices;

    private readonly List<LeaseFeeInterestWaiver> _leaseFeeInterestWaivers = [];

    /// <summary>Gets every late-fee interest waiver on this lease, in the order recorded.</summary>
    public IReadOnlyList<LeaseFeeInterestWaiver> LeaseFeeInterestWaivers => _leaseFeeInterestWaivers;

    private readonly List<PermittedMortgage> _permittedMortgages = [];

    /// <summary>Gets every mortgage the trust permitted on this lease, paid-off ones included, in the order recorded.</summary>
    public IReadOnlyList<PermittedMortgage> PermittedMortgages => _permittedMortgages;

    /// <summary>
    /// Creates a ground lease that didn't come from a purchase, such as one signed before the trust used
    /// CommonGround. Refused while the home is already leased, being sold, or in a home selection. Fees are tracked
    /// from the lease start unless <paramref name="leaseFeeTracking"/> says otherwise.
    /// </summary>
    public static GroundLeaseCreated Establish(
        GroundLeaseId id,
        string landParcelId,
        string unitId,
        Homeowner homeowner,
        DateOnly leaseStartDate,
        int termYears,
        int optionWindowDays,
        LeaseFeeTerms leaseFeeTerms,
        MortgageDebtCap mortgageDebtCap,
        decimal basePrice,
        decimal originalAppraisedValue,
        string covenantTerms,
        ResalePolicyTerms resalePolicyTerms,
        HomeCommitments homeCommitments,
        UserId createdBy,
        LeaseFeeTracking? leaseFeeTracking = null)
    {
        ArgumentNullException.ThrowIfNull(leaseFeeTerms);
        ArgumentNullException.ThrowIfNull(mortgageDebtCap);
        ArgumentNullException.ThrowIfNull(landParcelId);
        ArgumentNullException.ThrowIfNull(unitId);
        ArgumentNullException.ThrowIfNull(homeowner);
        ArgumentNullException.ThrowIfNull(resalePolicyTerms);
        ArgumentNullException.ThrowIfNull(homeCommitments);

        if (homeCommitments.ActiveLeaseId is { } activeLeaseId)
            throw BusinessRuleViolation.GroundLease.Create.HomeHasActiveLease(activeLeaseId);

        if (homeCommitments.OpenPurchaseWorkflowId is { } openPurchaseWorkflowId)
            throw BusinessRuleViolation.GroundLease.Create.HomeHasOpenPurchase(openPurchaseWorkflowId);

        if (homeCommitments.OpenHomeSelectionId is { } openHomeSelectionId)
            throw BusinessRuleViolation.GroundLease.Create.HomeInSelection(openHomeSelectionId);

        if (termYears <= 0)
            throw BusinessRuleViolation.GroundLease.Create.TermYearsNonPositive(termYears);

        if (optionWindowDays <= 0)
            throw BusinessRuleViolation.GroundLease.Create.OptionWindowDaysNonPositive(optionWindowDays);

        if (basePrice <= 0)
            throw BusinessRuleViolation.GroundLease.Create.BasePriceNonPositive(basePrice);

        if (originalAppraisedValue <= 0)
            throw BusinessRuleViolation.GroundLease.Create.OriginalAppraisedValueNonPositive(originalAppraisedValue);

        if (string.IsNullOrWhiteSpace(covenantTerms))
            throw BusinessRuleViolation.GroundLease.Create.CovenantTermsRequired();

        var tracking = leaseFeeTracking ?? LeaseFeeTracking.FromLeaseStart(leaseStartDate);
        if (tracking.TrackedFrom < leaseStartDate)
            throw BusinessRuleViolation.LeaseFeeTracking.Create.TrackedFromBeforeLeaseStart(tracking.TrackedFrom, leaseStartDate);

        return new GroundLeaseCreated(
            id.Value,
            landParcelId,
            unitId,
            leaseStartDate,
            termYears,
            optionWindowDays,
            leaseFeeTerms.LandUseFee,
            leaseFeeTerms.RepairReserveFee,
            leaseFeeTerms.LateInterestRate,
            leaseFeeTerms.InterestWaiverDays,
            mortgageDebtCap.Percent,
            tracking.TrackedFrom,
            tracking.OpeningBalance,
            basePrice,
            originalAppraisedValue,
            covenantTerms,
            resalePolicyTerms.FormulaKind,
            resalePolicyTerms.AppreciationShare,
            resalePolicyTerms.IndexType,
            resalePolicyTerms.BaselineIndexValue,
            resalePolicyTerms.AnnualPercentage,
            resalePolicyTerms.ImprovementsCap,
            createdBy.Value,
            homeowner.FullName.Value,
            homeowner.Email.Value,
            homeowner.Phone?.Value);
    }

    /// <summary>
    /// Creates a new ground lease from an active template when a purchase closes. The purchase loan becomes the
    /// lease's first permitted mortgage, permitted by signing the lease (Art 8.2), with no notice address yet.
    /// </summary>
    public static GroundLeaseCreatedFromPurchase CreateFromPurchase(
        GroundLeaseId id,
        GroundLeaseTemplate template,
        PurchaseWorkflow purchaseWorkflow,
        DateOnly leaseStartDate,
        PermittedMortgageId purchaseMortgageId,
        UserId createdBy)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(purchaseWorkflow);

        if (template.Status != GroundLeaseTemplateStatus.Active)
        {
            throw BusinessRuleViolation.GroundLease.CreateFromTemplate.TemplateNotActive(template.Status);
        }

        // The financing step doesn't refuse a zero amount or a fraction of a cent, and a refusal here would stop the
        // lease being created at closing; a zero commitment is no loan.
        var purchaseLoan = purchaseWorkflow.FinancingDetails is { CommittedAmount: > 0 } financing ? financing : null;

        return new GroundLeaseCreatedFromPurchase(
            id.Value,
            purchaseWorkflow.Id.Value,
            template.Id.Value,
            purchaseWorkflow.LandParcelId,
            purchaseWorkflow.HomeId,
            leaseStartDate,
            template.TermYears,
            template.OptionWindowDays,
            template.LeaseFeeTerms.LandUseFee,
            template.LeaseFeeTerms.RepairReserveFee,
            template.LeaseFeeTerms.LateInterestRate,
            template.LeaseFeeTerms.InterestWaiverDays,
            template.MortgageDebtCap.Percent,
            purchaseWorkflow.PurchasePrice,
            purchaseWorkflow.AppraisalDetails?.AppraisedValue,
            template.CovenantTerms,
            template.ResalePolicyTerms.FormulaKind,
            template.ResalePolicyTerms.AppreciationShare,
            template.ResalePolicyTerms.IndexType,
            template.ResalePolicyTerms.BaselineIndexValue,
            template.ResalePolicyTerms.AnnualPercentage,
            template.ResalePolicyTerms.ImprovementsCap,
            createdBy.Value,
            purchaseWorkflow.Buyer.FullName.Value,
            purchaseWorkflow.Buyer.Email.Value,
            purchaseWorkflow.Buyer.Phone?.Value,
            purchaseLoan is null ? null : purchaseMortgageId.Value,
            purchaseLoan?.LenderName,
            purchaseLoan is null ? null : Money.RoundedFrom(purchaseLoan.CommittedAmount));
    }

    /// <summary>Terminates this ground lease.</summary>
    /// <param name="reason">The reason for termination.</param>
    /// <param name="terminatedDate">The date of termination.</param>
    /// <param name="terminatedBy">The user recording this action.</param>
    public GroundLeaseTerminated Terminate(string reason, DateOnly terminatedDate, UserId terminatedBy)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw BusinessRuleViolation.GroundLease.Terminate.ReasonRequired();

        if (Status != GroundLeaseStatus.Active)
            throw BusinessRuleViolation.GroundLease.Terminate.StatusNotActive(Status);

        return new GroundLeaseTerminated(
            Id.Value,
            reason,
            terminatedDate,
            terminatedBy.Value);
    }

    /// <summary>Records a stewardship check-in visit.</summary>
    /// <param name="checkInDate">Date of the check-in.</param>
    /// <param name="notes">Notes from the visit.</param>
    /// <param name="recordedBy">The user recording this action.</param>
    public StewardshipCheckInRecorded RecordStewardshipCheckIn(DateOnly checkInDate, string notes, UserId recordedBy)
    {
        if (string.IsNullOrWhiteSpace(notes))
        {
            throw BusinessRuleViolation.GroundLease.RecordStewardshipCheckIn.NotesRequired();
        }

        return new StewardshipCheckInRecorded(
            Id.Value,
            checkInDate,
            notes,
            recordedBy.Value);
    }

    /// <summary>
    /// Starts a resale on this lease when its homeowner gives notice to sell. Refused unless the lease is
    /// active, and while another resale is open on it.
    /// </summary>
    public ResaleStarted StartResale(ResaleId resaleId)
    {
        if (Status != GroundLeaseStatus.Active)
            throw BusinessRuleViolation.GroundLease.StartResale.StatusNotActive(Status);

        if (OpenResaleId is { } openResaleId)
            throw BusinessRuleViolation.GroundLease.StartResale.ResaleAlreadyOpen(openResaleId.Value);

        return new ResaleStarted(Id.Value, resaleId.Value);
    }

    /// <summary>Closes the lease's open resale, so its homeowner can give notice again. Nothing to do if it isn't open.</summary>
    public ResaleClosed? CloseResale(ResaleId resaleId) =>
        OpenResaleId == resaleId ? new ResaleClosed(Id.Value, resaleId.Value) : null;

    /// <summary>
    /// Records a lease fee payment the trust received. Refused for an amount of zero or less, a received date in the
    /// future or before the lease started. An ended lease still takes payments, so arrears can be collected.
    /// </summary>
    public LeaseFeePaymentRecorded RecordLeaseFeePayment(
        LeaseFeePaymentId paymentId,
        DateOnly receivedDate,
        Money amount,
        PaymentMethod method,
        string? reference,
        DateOnly today,
        UserId recordedBy)
    {
        ArgumentNullException.ThrowIfNull(amount);
        ArgumentNullException.ThrowIfNull(method);

        if (!amount.IsPositive)
            throw BusinessRuleViolation.GroundLease.RecordLeaseFeePayment.AmountNonPositive(amount.Amount);

        if (receivedDate > today)
            throw BusinessRuleViolation.GroundLease.RecordLeaseFeePayment.ReceivedDateInFuture(receivedDate, today);

        if (receivedDate < LeaseFeeTracking.TrackedFrom)
            throw BusinessRuleViolation.GroundLease.RecordLeaseFeePayment.ReceivedDateBeforeFeesTracked(receivedDate, LeaseFeeTracking.TrackedFrom);

        return new LeaseFeePaymentRecorded(
            Id.Value,
            paymentId.Value,
            receivedDate,
            amount,
            method,
            string.IsNullOrWhiteSpace(reference) ? null : reference.Trim(),
            recordedBy.Value);
    }

    /// <summary>
    /// Voids a payment recorded by mistake, so it no longer counts toward the balance. Refused without a reason,
    /// for a payment not on this lease, or one already voided.
    /// </summary>
    public LeaseFeePaymentVoided VoidLeaseFeePayment(LeaseFeePaymentId paymentId, string reason, UserId voidedBy)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw BusinessRuleViolation.GroundLease.VoidLeaseFeePayment.ReasonRequired();

        var payment = _leaseFeePayments.FirstOrDefault(p => p.Id == paymentId)
            ?? throw BusinessRuleViolation.GroundLease.VoidLeaseFeePayment.PaymentNotFound(paymentId.Value);

        if (payment.IsVoided)
            throw BusinessRuleViolation.GroundLease.VoidLeaseFeePayment.AlreadyVoided(paymentId.Value);

        return new LeaseFeePaymentVoided(Id.Value, paymentId.Value, reason.Trim(), voidedBy.Value);
    }

    /// <summary>
    /// Corrects the day the lease's fees are tracked from and what was owed on it, such as after an import with the
    /// wrong date. Refused for a day before the lease started, an opening balance below zero, or a day after a
    /// payment that still counts: void that payment first.
    /// </summary>
    public LeaseFeeTrackingCorrected CorrectLeaseFeeTracking(DateOnly feesTrackedFrom, Money openingBalance, UserId correctedBy)
    {
        var tracking = LeaseFeeTracking.Create(feesTrackedFrom, openingBalance, LeaseStartDate);

        var earliestPayment = _leaseFeePayments.Where(p => !p.IsVoided).Select(p => (DateOnly?)p.ReceivedDate).Min();
        if (earliestPayment < tracking.TrackedFrom)
            throw BusinessRuleViolation.GroundLease.CorrectLeaseFeeTracking.PaymentBeforeFeesTracked(earliestPayment.Value, tracking.TrackedFrom);

        return new LeaseFeeTrackingCorrected(Id.Value, tracking.TrackedFrom, tracking.OpeningBalance, correctedBy.Value);
    }

    /// <summary>
    /// Records a late notice the trust sent, keeping the balance and oldest unpaid month on the day it was sent.
    /// Refused for a sent date in the future or before the lease started, or when nothing was owed that day. A
    /// notice can be recorded before it falls due, and on an ended lease. A notice of default also keeps the trust's
    /// cure period, and is refused unless delivered as the notice clause requires (Art 14.2), or while an earlier
    /// notice of default's cure period is still running.
    /// </summary>
    public LeaseFeeNoticeRecorded RecordLeaseFeeNotice(
        LeaseFeeNoticeId noticeId,
        LeaseFeeNoticeLevel level,
        DateOnly sentOn,
        NoticeDelivery delivery,
        DefaultCurePeriod curePeriod,
        DateOnly today,
        UserId recordedBy)
    {
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(delivery);
        ArgumentNullException.ThrowIfNull(curePeriod);

        if (sentOn > today)
            throw BusinessRuleViolation.GroundLease.RecordLeaseFeeNotice.SentDateInFuture(sentOn, today);

        if (sentOn < LeaseStartDate)
            throw BusinessRuleViolation.GroundLease.RecordLeaseFeeNotice.SentDateBeforeLeaseStart(sentOn, LeaseStartDate);

        var statement = LeaseFeeStatementAsOf(sentOn);
        if (!statement.Balance.IsPositive || statement.OldestUnpaidDueDate is not { } oldestUnpaid)
            throw BusinessRuleViolation.GroundLease.RecordLeaseFeeNotice.NothingOwed(sentOn);

        if (level != LeaseFeeNoticeLevel.Default)
        {
            return new LeaseFeeNoticeRecorded(
                Id.Value, noticeId.Value, level, sentOn, delivery, statement.Balance, oldestUnpaid, recordedBy.Value);
        }

        if (!delivery.MeetsNoticeClause)
            throw BusinessRuleViolation.GroundLease.RecordLeaseFeeNotice.DefaultNoticeDeliveryNotAllowed(delivery);

        if (DefaultCureAsOf(sentOn) is { Status: var status, CureBy: var cureBy } && status == DefaultCureStatus.Curing)
            throw BusinessRuleViolation.GroundLease.RecordLeaseFeeNotice.DefaultNoticeAlreadyCuring(cureBy);

        return new LeaseFeeNoticeRecorded(
            Id.Value, noticeId.Value, level, sentOn, delivery, statement.Balance, oldestUnpaid, recordedBy.Value,
            curePeriod.CureDays, curePeriod.ExtensionDays);
    }

    /// <summary>
    /// Waives late-fee interest owed today, such as for a household in hardship (Art 5.4). Refused without a reason,
    /// for an amount of zero or less, or one above the interest owed today. An ended lease's interest can be waived too.
    /// </summary>
    public LeaseFeeInterestWaived WaiveLeaseFeeInterest(
        LeaseFeeInterestWaiverId waiverId,
        Money amount,
        string reason,
        DateOnly today,
        UserId waivedBy)
    {
        ArgumentNullException.ThrowIfNull(amount);

        if (string.IsNullOrWhiteSpace(reason))
            throw BusinessRuleViolation.GroundLease.WaiveLeaseFeeInterest.ReasonRequired();

        if (!amount.IsPositive)
            throw BusinessRuleViolation.GroundLease.WaiveLeaseFeeInterest.AmountNonPositive(amount.Amount);

        var interestDue = LeaseFeeStatementAsOf(today).InterestDue;
        if (amount > interestDue)
            throw BusinessRuleViolation.GroundLease.WaiveLeaseFeeInterest.AmountAboveInterestOwed(amount.Amount, interestDue.Amount);

        return new LeaseFeeInterestWaived(Id.Value, waiverId.Value, today, amount, reason.Trim(), waivedBy.Value);
    }

    /// <summary>
    /// Records a mortgage the trust permitted outside CommonGround, such as one on a lease created before the trust
    /// used it (Art 8.1). Refused without a lender, for a principal of zero or less, or a permitted date in the future
    /// or before the lease started. An ended lease takes one too, to complete its record.
    /// </summary>
    public PermittedMortgageRecorded RecordPermittedMortgage(
        PermittedMortgageId mortgageId,
        string lender,
        string? noticeAddress,
        Money originalPrincipal,
        DateOnly permittedOn,
        DateOnly today,
        UserId recordedBy)
    {
        ArgumentNullException.ThrowIfNull(originalPrincipal);

        if (string.IsNullOrWhiteSpace(lender))
            throw BusinessRuleViolation.GroundLease.RecordPermittedMortgage.LenderRequired();

        if (!originalPrincipal.IsPositive)
            throw BusinessRuleViolation.GroundLease.RecordPermittedMortgage.PrincipalNonPositive(originalPrincipal.Amount);

        if (permittedOn > today)
            throw BusinessRuleViolation.GroundLease.RecordPermittedMortgage.PermittedOnInFuture(permittedOn, today);

        if (permittedOn < LeaseStartDate)
            throw BusinessRuleViolation.GroundLease.RecordPermittedMortgage.PermittedOnBeforeLeaseStart(permittedOn, LeaseStartDate);

        return new PermittedMortgageRecorded(
            Id.Value,
            mortgageId.Value,
            lender.Trim(),
            OptionalText(noticeAddress),
            originalPrincipal,
            permittedOn,
            MortgagePermissionSource.Recorded,
            recordedBy.Value);
    }

    /// <summary>
    /// Changes a permitted mortgage's lender and notice address, such as when servicing moves to another company.
    /// Refused without a lender, for a mortgage not on this lease, or one paid off. Nothing to do if both are unchanged.
    /// </summary>
    public PermittedMortgageEdited? EditPermittedMortgage(
        PermittedMortgageId mortgageId,
        string lender,
        string? noticeAddress,
        UserId editedBy)
    {
        if (string.IsNullOrWhiteSpace(lender))
            throw BusinessRuleViolation.GroundLease.EditPermittedMortgage.LenderRequired();

        var mortgage = _permittedMortgages.FirstOrDefault(m => m.Id == mortgageId)
            ?? throw BusinessRuleViolation.GroundLease.EditPermittedMortgage.MortgageNotFound(mortgageId.Value);

        if (mortgage.IsPaidOff)
            throw BusinessRuleViolation.GroundLease.EditPermittedMortgage.MortgagePaidOff(mortgageId.Value);

        var newLender = lender.Trim();
        var newAddress = OptionalText(noticeAddress);
        if (newLender == mortgage.Lender && newAddress == mortgage.NoticeAddress)
            return null;

        return new PermittedMortgageEdited(Id.Value, mortgageId.Value, newLender, newAddress, editedBy.Value);
    }

    /// <summary>
    /// Marks a permitted mortgage paid off, so it no longer gets copies of notices. Refused for a mortgage not on this
    /// lease, one already paid off, or a date in the future or before the trust permitted it.
    /// </summary>
    public PermittedMortgagePaidOff MarkPermittedMortgagePaidOff(
        PermittedMortgageId mortgageId,
        DateOnly paidOffOn,
        DateOnly today,
        UserId recordedBy)
    {
        var mortgage = _permittedMortgages.FirstOrDefault(m => m.Id == mortgageId)
            ?? throw BusinessRuleViolation.GroundLease.MarkPermittedMortgagePaidOff.MortgageNotFound(mortgageId.Value);

        if (mortgage.IsPaidOff)
            throw BusinessRuleViolation.GroundLease.MarkPermittedMortgagePaidOff.AlreadyPaidOff(mortgageId.Value);

        if (paidOffOn > today)
            throw BusinessRuleViolation.GroundLease.MarkPermittedMortgagePaidOff.PaidOffOnInFuture(paidOffOn, today);

        if (paidOffOn < mortgage.PermittedOn)
            throw BusinessRuleViolation.GroundLease.MarkPermittedMortgagePaidOff.PaidOffOnBeforePermitted(paidOffOn, mortgage.PermittedOn);

        return new PermittedMortgagePaidOff(Id.Value, mortgageId.Value, paidOffOn, recordedBy.Value);
    }

    /// <summary>
    /// Adds a loan the trust approved on the homeowner's request, once it closed, as a permitted mortgage from its
    /// approval date, and marks the mortgages it paid off as paid off on the closing date. Refused for a paid-off
    /// mortgage not on this lease, already paid off, or permitted after the closing date.
    /// </summary>
    /// <param name="mortgageId">The new permitted mortgage's ID.</param>
    /// <param name="request">The approved request on this lease.</param>
    /// <param name="closedOn">The day the loan closed.</param>
    /// <param name="paidOffMortgageIds">The lease's mortgages the loan paid off.</param>
    /// <param name="today">The trust's today.</param>
    /// <param name="recordedBy">The staff user recording it.</param>
    public IReadOnlyList<IDomainEvent> PermitRequestedMortgage(
        PermittedMortgageId mortgageId,
        MortgageRequest request,
        DateOnly closedOn,
        IReadOnlyList<PermittedMortgageId> paidOffMortgageIds,
        DateOnly today,
        UserId recordedBy)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(paidOffMortgageIds);

        if (request.GroundLeaseId != Id)
            throw new ArgumentException("The request isn't on this lease.", nameof(request));

        var approvedOn = request.ApprovedOn
            ?? throw new ArgumentException("The request isn't approved.", nameof(request));

        IReadOnlyList<IDomainEvent> paidOff = [.. paidOffMortgageIds
            .Distinct()
            .Select(id => MarkPermittedMortgagePaidOff(id, closedOn, today, recordedBy))];

        return
        [
            new PermittedMortgageRecorded(
                Id.Value,
                mortgageId.Value,
                request.Terms.Lender,
                null,
                request.Terms.Principal,
                approvedOn,
                MortgagePermissionSource.Request,
                recordedBy.Value),
            .. paidOff,
        ];
    }

    private static string? OptionalText(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();

    /// <summary>Works out what the lease owes as of a day, usually the trust's today.</summary>
    public LeaseFeeStatement LeaseFeeStatementAsOf(DateOnly asOf) =>
        LeaseFeeStatement.Calculate(
            LeaseFeeTerms,
            LeaseFeeTracking,
            EndedOn,
            CountedPayments(),
            asOf,
            _leaseFeeInterestWaivers.Select(w => new WaivedInterest(w.WaivedOn, w.Amount)));

    /// <summary>Gets where the lease's last notice of default sent on or before a day stands that day, or null.</summary>
    public DefaultCure? DefaultCureAsOf(DateOnly asOf) =>
        _leaseFeeNotices
            .Where(n => n.Level == LeaseFeeNoticeLevel.Default && n.SentOn <= asOf)
            .MaxBy(n => n.SentOn)
            ?.CureAsOf(CountedPayments(), asOf);

    private IEnumerable<ReceivedPayment> CountedPayments() =>
        _leaseFeePayments.Where(p => !p.IsVoided).Select(p => new ReceivedPayment(p.ReceivedDate, p.Amount));

    /// <summary>
    /// Gets the late notice the lease needs as of a day, or null. Only an active lease needs one; an ended lease's
    /// arrears are collected from the sale (Art 5.8), not by letter.
    /// </summary>
    public LeaseFeeNoticeLevel? LeaseFeeNoticeDueAsOf(DateOnly asOf, LeaseFeeNoticeSchedule schedule)
    {
        ArgumentNullException.ThrowIfNull(schedule);

        return Status != GroundLeaseStatus.Active
            ? null
            : schedule.NoticeDue(
                LeaseFeeStatementAsOf(asOf),
                _leaseFeeNotices.Select(n => new LeaseFeeNoticeSent(n.Level, n.SentOn)));
    }

    /// <summary>Starts a ground lease from its first event.</summary>
    public static GroundLease Create(GroundLeaseCreated e)
    {
        ArgumentNullException.ThrowIfNull(e);

        var groundLease = new GroundLease();
        groundLease.Apply(e);
        return groundLease;
    }

    private void Apply(GroundLeaseCreated e)
    {
        Id = GroundLeaseId.Create(e.AggregateId);
        LandParcelId = e.LandParcelId;
        UnitId = e.HomeId;
        LeaseStartDate = e.LeaseStartDate;
        TermYears = e.TermYears;
        OptionWindowDays = e.OptionWindowDays;
        LeaseFeeTerms = LeaseFeeTerms.Create(e.LandUseFee, e.RepairReserveFee, e.LateInterestRate, e.InterestWaiverDays);
        LeaseFeeTracking = LeaseFeeTracking.Create(e.FeesTrackedFrom, e.OpeningBalance, e.LeaseStartDate);
        MortgageDebtCap = MortgageDebtCap.Create(e.MaxMortgageDebtPercent);
        BasePrice = e.BasePrice;
        OriginalAppraisedValue = e.OriginalAppraisedValue;
        CovenantTerms = e.CovenantTerms;
        ResalePolicyTerms = e.FormulaKind switch
        {
            FormulaKind.AppraisalBased => ResalePolicyTerms.AppraisalBased(
                e.AppreciationShare!.Value, e.ImprovementsCap),
            FormulaKind.IndexBased => ResalePolicyTerms.IndexBased(
                e.IndexType!, e.BaselineIndexValue!.Value, e.ImprovementsCap),
            FormulaKind.FixedPercentage => ResalePolicyTerms.FixedPercentage(
                e.AnnualPercentage!.Value, e.ImprovementsCap),
            _ => throw new InvalidOperationException($"Unknown formula kind: {e.FormulaKind}")
        };
        Status = GroundLeaseStatus.Active;
        CreatedBy = UserId.Create(e.RegisteredBy);
        CreatedAt = e.OccurredAt;
        TemplateId = null;
        CurrentHomeowner = Homeowner.From(e.HomeownerFullName, e.HomeownerEmail, e.HomeownerPhone);
    }

    /// <summary>Starts a ground lease from its first event.</summary>
    public static GroundLease Create(GroundLeaseCreatedFromPurchase e)
    {
        ArgumentNullException.ThrowIfNull(e);

        var groundLease = new GroundLease();
        groundLease.Apply(e);
        return groundLease;
    }

    private void Apply(GroundLeaseCreatedFromPurchase e)
    {
        Id = GroundLeaseId.Create(e.AggregateId);
        PurchaseWorkflowId = PurchaseWorkflows.PurchaseWorkflowId.Create(e.PurchaseWorkflowId);
        TemplateId = GroundLeaseTemplateId.Create(e.GroundLeaseTemplateId);
        LandParcelId = e.LandParcelId;
        UnitId = e.HomeId;
        LeaseStartDate = e.LeaseStartDate;
        TermYears = e.TermYears;
        OptionWindowDays = e.OptionWindowDays;
        LeaseFeeTerms = LeaseFeeTerms.Create(e.LandUseFee, e.RepairReserveFee, e.LateInterestRate, e.InterestWaiverDays);
        LeaseFeeTracking = LeaseFeeTracking.FromLeaseStart(e.LeaseStartDate);
        MortgageDebtCap = MortgageDebtCap.Create(e.MaxMortgageDebtPercent);
        if (e.PurchaseMortgageId is { } purchaseMortgageId)
        {
            _permittedMortgages.Add(PermittedMortgage.Create(
                PermittedMortgageId.Create(purchaseMortgageId), e.PurchaseLender!, null, e.PurchaseLoanPrincipal!,
                e.LeaseStartDate, MortgagePermissionSource.LeaseSigning));
        }
        BasePrice = e.BasePrice;
        OriginalAppraisedValue = e.OriginalAppraisedValue;
        CovenantTerms = e.CovenantTerms;
        ResalePolicyTerms = e.FormulaKind switch
        {
            FormulaKind.AppraisalBased => ResalePolicyTerms.AppraisalBased(
                e.AppreciationShare!.Value, e.ImprovementsCap),
            FormulaKind.IndexBased => ResalePolicyTerms.IndexBased(
                e.IndexType!, e.BaselineIndexValue!.Value, e.ImprovementsCap),
            FormulaKind.FixedPercentage => ResalePolicyTerms.FixedPercentage(
                e.AnnualPercentage!.Value, e.ImprovementsCap),
            _ => throw new InvalidOperationException($"Unknown formula kind: {e.FormulaKind}")
        };
        Status = GroundLeaseStatus.Active;
        CreatedBy = UserId.Create(e.RegisteredBy);
        CreatedAt = e.OccurredAt;
        CurrentHomeowner = Homeowner.From(e.BuyerFullName, e.BuyerEmail, e.BuyerPhone);
    }

    /// <summary>Applies <see cref="ResaleStarted"/>.</summary>
    public void Apply(ResaleStarted e) => OpenResaleId = ResaleId.Create(e.ResaleId);

    /// <summary>Applies <see cref="ResaleClosed"/>.</summary>
    public void Apply(ResaleClosed _) => OpenResaleId = null;

    /// <summary>Applies <see cref="GroundLeaseTerminated"/>.</summary>
    public void Apply(GroundLeaseTerminated e)
    {
        Status = GroundLeaseStatus.Inactive;
        EndedOn = e.TerminatedDate;
    }

    /// <summary>Applies <see cref="LeaseFeePaymentRecorded"/>.</summary>
    public void Apply(LeaseFeePaymentRecorded e) =>
        _leaseFeePayments.Add(LeaseFeePayment.Create(
            LeaseFeePaymentId.Create(e.PaymentId), e.ReceivedDate, e.Amount, e.Method, e.Reference));

    /// <summary>Applies <see cref="LeaseFeeNoticeRecorded"/>.</summary>
    public void Apply(LeaseFeeNoticeRecorded e) =>
        _leaseFeeNotices.Add(LeaseFeeNotice.Create(
            LeaseFeeNoticeId.Create(e.NoticeId), e.Level, e.SentOn, e.Delivery, e.BalanceDue, e.OldestUnpaidDueDate,
            e.CureDays is { } cureDays ? DefaultCurePeriod.Create(cureDays, e.CureExtensionDays ?? 0) : null));

    /// <summary>Applies <see cref="LeaseFeeInterestWaived"/>.</summary>
    public void Apply(LeaseFeeInterestWaived e) =>
        _leaseFeeInterestWaivers.Add(LeaseFeeInterestWaiver.Create(
            LeaseFeeInterestWaiverId.Create(e.WaiverId), e.WaivedOn, e.Amount, e.Reason));

    /// <summary>Applies <see cref="LeaseFeeTrackingCorrected"/>.</summary>
    public void Apply(LeaseFeeTrackingCorrected e) =>
        LeaseFeeTracking = LeaseFeeTracking.Create(e.FeesTrackedFrom, e.OpeningBalance, LeaseStartDate);

    /// <summary>Applies <see cref="LeaseFeePaymentVoided"/>.</summary>
    public void Apply(LeaseFeePaymentVoided e) =>
        _leaseFeePayments.First(p => p.Id.Value == e.PaymentId).MarkVoided(e.Reason);

    /// <summary>Applies <see cref="PermittedMortgageRecorded"/>.</summary>
    public void Apply(PermittedMortgageRecorded e) =>
        _permittedMortgages.Add(PermittedMortgage.Create(
            PermittedMortgageId.Create(e.MortgageId), e.Lender, e.NoticeAddress, e.OriginalPrincipal, e.PermittedOn, e.Source));

    /// <summary>Applies <see cref="PermittedMortgageEdited"/>.</summary>
    public void Apply(PermittedMortgageEdited e) =>
        _permittedMortgages.First(m => m.Id.Value == e.MortgageId).ChangeLender(e.Lender, e.NoticeAddress);

    /// <summary>Applies <see cref="PermittedMortgagePaidOff"/>.</summary>
    public void Apply(PermittedMortgagePaidOff e) =>
        _permittedMortgages.First(m => m.Id.Value == e.MortgageId).MarkPaidOff(e.PaidOffOn);

}
