using CommonGround.Homeownership.Application.Errors;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.Mortgages;
using CommonGround.Homeownership.Domain.MortgageRequests.Events;
using CommonGround.Homeownership.Domain.Resales;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.MortgageRequests;

/// <summary>
/// A homeowner's request to put a new loan on the home: a refinance, a second mortgage, a repair loan (Model Lease
/// Art 8.3). The trust compares the total debt the loan leaves with the lease's debt cap, then approves or denies it.
/// An approved loan becomes a permitted mortgage on the lease when it closes. A lease has one pending request at a time.
/// </summary>
public sealed class MortgageRequest : AggregateRoot<MortgageRequestId>
{
    /// <summary>The business days before closing the lease asks the homeowner to send a request (Art 8.3).</summary>
    public const int RequiredNoticeBusinessDays = 15;

#pragma warning disable CS8618 // State is set by Create from the stream's first event.
    private MortgageRequest() { }
#pragma warning restore CS8618

    /// <summary>Gets the homeowner's lease.</summary>
    public GroundLeaseId GroundLeaseId { get; private set; }

    /// <summary>Gets where the request stands.</summary>
    public MortgageRequestStatus Status { get; private set; }

    /// <summary>Gets the day the request reached the trust.</summary>
    public DateOnly ReceivedOn { get; private set; }

    /// <summary>Gets the loan's terms.</summary>
    public MortgageLoanTerms Terms { get; private set; }

    /// <summary>Gets the day the loan is expected to close.</summary>
    public DateOnly ExpectedClosingOn { get; private set; }

    /// <summary>Gets the loan's closing costs, or null while not known.</summary>
    public Money? ClosingCosts { get; private set; }

    /// <summary>Gets the loan's appraisal, or null while not received.</summary>
    public LoanAppraisal? Appraisal { get; private set; }

    /// <summary>Gets the debt check from the latest appraisal, or null while there is none.</summary>
    public MortgageDebtCheck? DebtCheck { get; private set; }

    /// <summary>Gets the day the trust approved the loan, if it has.</summary>
    public DateOnly? ApprovedOn { get; private set; }

    /// <summary>
    /// Counts the business days (Monday to Friday) after <paramref name="from"/> up to and including
    /// <paramref name="to"/>. Public holidays count as business days.
    /// </summary>
    public static int BusinessDaysBetween(DateOnly from, DateOnly to)
    {
        var days = 0;
        for (var day = from.AddDays(1); day <= to; day = day.AddDays(1))
        {
            if (day.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday))
                days++;
        }

        return days;
    }

    /// <summary>
    /// Records a homeowner's request for a new loan on the home. Refused unless the lease is active, while another
    /// request on the lease is pending, for a received date in the future or before the lease started, an expected
    /// closing before the received date, or negative closing costs. When the loan's appraisal came with the request,
    /// it is recorded too, with its debt check.
    /// </summary>
    /// <param name="id">The new request's ID.</param>
    /// <param name="lease">The homeowner's lease.</param>
    /// <param name="pendingOnLease">The lease's pending request, if it has one.</param>
    /// <param name="receivedOn">The day the request reached the trust.</param>
    /// <param name="terms">The loan's terms.</param>
    /// <param name="expectedClosingOn">The day the loan is expected to close.</param>
    /// <param name="closingCosts">The loan's closing costs, if known.</param>
    /// <param name="appraisal">The loan's appraisal, if it came with the request.</param>
    /// <param name="notes">Anything staff want to note.</param>
    /// <param name="today">The trust's today.</param>
    /// <param name="receivedBy">The staff user recording it.</param>
    public static IReadOnlyList<IDomainEvent> Receive(
        MortgageRequestId id,
        GroundLease lease,
        MortgageRequestId? pendingOnLease,
        DateOnly receivedOn,
        MortgageLoanTerms terms,
        DateOnly expectedClosingOn,
        Money? closingCosts,
        LoanAppraisal? appraisal,
        string? notes,
        DateOnly today,
        UserId receivedBy)
    {
        ArgumentNullException.ThrowIfNull(lease);
        ArgumentNullException.ThrowIfNull(terms);

        if (lease.Status != GroundLeaseStatus.Active)
            throw BusinessRuleViolation.MortgageRequest.Receive.LeaseNotActive(lease.Status);

        if (pendingOnLease is { } pending)
            throw BusinessRuleViolation.MortgageRequest.Receive.RequestPending(pending.Value);

        if (receivedOn > today)
            throw BusinessRuleViolation.MortgageRequest.Receive.ReceivedOnInFuture(receivedOn, today);

        if (receivedOn < lease.LeaseStartDate)
            throw BusinessRuleViolation.MortgageRequest.Receive.ReceivedOnBeforeLeaseStart(receivedOn, lease.LeaseStartDate);

        if (expectedClosingOn < receivedOn)
            throw BusinessRuleViolation.MortgageRequest.Receive.ExpectedClosingBeforeReceived(expectedClosingOn, receivedOn);

        CheckClosingCosts(closingCosts);

        var received = new MortgageRequestReceived(
            id.Value,
            lease.Id.Value,
            lease.UnitId,
            lease.CurrentHomeowner?.FullName.Value,
            receivedOn,
            terms.Lender,
            terms.Purpose,
            terms.Principal,
            terms.TotalDebtAfterClosing,
            terms.InterestRatePercent,
            terms.RateType,
            terms.TermYears,
            expectedClosingOn,
            closingCosts,
            string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            receivedBy.Value);

        return appraisal is null
            ? [received]
            : [received, Appraised(id, lease, terms.TotalDebtAfterClosing, appraisal, closingCosts, today, receivedBy)];
    }

    /// <summary>
    /// Records the loan's appraisal and closing costs while the request waits for a decision, and works out the debt
    /// check: the lease's formula gives the purchase option price from the appraisal (no approved improvements), and
    /// the cap is the lease's cap percent of it. A later appraisal replaces it. Refused for an appraisal date in the
    /// future, or when the lease has no original appraised value for the formula.
    /// </summary>
    public MortgageRequestAppraisalRecorded RecordAppraisal(
        GroundLease lease,
        LoanAppraisal appraisal,
        Money? closingCosts,
        DateOnly today,
        UserId recordedBy)
    {
        ArgumentNullException.ThrowIfNull(lease);
        ArgumentNullException.ThrowIfNull(appraisal);

        if (lease.Id != GroundLeaseId)
            throw new ArgumentException("The lease isn't the request's lease.", nameof(lease));

        if (Status != MortgageRequestStatus.Pending)
            throw BusinessRuleViolation.MortgageRequest.RecordAppraisal.NotPending(Status);

        CheckClosingCosts(closingCosts);

        return Appraised(Id, lease, Terms.TotalDebtAfterClosing, appraisal, closingCosts, today, recordedBy);
    }

    /// <summary>
    /// Gives the trust's written permission for the loan (Art 8.1). Needs the appraisal's debt check. A loan above the
    /// cap may be approved with a reason, such as a refinance that lowers the payment on the first loan. Refused for an
    /// approved date before the request arrived or in the future.
    /// </summary>
    public MortgageRequestApproved Approve(DateOnly approvedOn, string? reason, DateOnly today, UserId approvedBy)
    {
        if (Status != MortgageRequestStatus.Pending)
            throw BusinessRuleViolation.MortgageRequest.Approve.NotPending(Status);

        if (DebtCheck is not { } check)
            throw BusinessRuleViolation.MortgageRequest.Approve.AppraisalRequired();

        CheckDecisionDate(approvedOn, today,
            BusinessRuleViolation.MortgageRequest.Approve.ApprovedOnBeforeReceived,
            BusinessRuleViolation.MortgageRequest.Approve.ApprovedOnInFuture);

        if (check.IsOverCap && string.IsNullOrWhiteSpace(reason))
            throw BusinessRuleViolation.MortgageRequest.Approve.OverCapReasonRequired(check.DebtPercent, check.Cap.Percent);

        return new MortgageRequestApproved(
            Id.Value,
            GroundLeaseId.Value,
            approvedOn,
            check.PurchaseOptionPrice,
            check.Cap.Percent,
            check.CapAmount,
            check.TotalDebt,
            check.DebtPercent,
            check.IsOverCap,
            string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
            approvedBy.Value);
    }

    /// <summary>
    /// Refuses the loan, with the reason the denial letter gives. Refused for a denied date before the request arrived
    /// or in the future.
    /// </summary>
    public MortgageRequestDenied Deny(DateOnly deniedOn, string reason, DateOnly today, UserId deniedBy)
    {
        if (Status != MortgageRequestStatus.Pending)
            throw BusinessRuleViolation.MortgageRequest.Deny.NotPending(Status);

        if (string.IsNullOrWhiteSpace(reason))
            throw BusinessRuleViolation.MortgageRequest.Deny.ReasonRequired();

        CheckDecisionDate(deniedOn, today,
            BusinessRuleViolation.MortgageRequest.Deny.DeniedOnBeforeReceived,
            BusinessRuleViolation.MortgageRequest.Deny.DeniedOnInFuture);

        return new MortgageRequestDenied(Id.Value, GroundLeaseId.Value, deniedOn, reason.Trim(), deniedBy.Value);
    }

    /// <summary>
    /// Records that the approved loan closed. The caller adds it to the lease as a permitted mortgage with
    /// <see cref="GroundLease.PermitRequestedMortgage"/>, which also marks the mortgages it paid off. Refused for a
    /// closing date before the approval or in the future.
    /// </summary>
    public MortgageRequestLoanClosed RecordLoanClosed(
        DateOnly closedOn,
        PermittedMortgageId permittedMortgageId,
        IReadOnlyList<PermittedMortgageId> paidOffMortgageIds,
        DateOnly today,
        UserId recordedBy)
    {
        ArgumentNullException.ThrowIfNull(paidOffMortgageIds);

        if (Status != MortgageRequestStatus.Approved || ApprovedOn is not { } approvedOn)
            throw BusinessRuleViolation.MortgageRequest.RecordLoanClosed.NotApproved(Status);

        if (closedOn < approvedOn)
            throw BusinessRuleViolation.MortgageRequest.RecordLoanClosed.ClosedOnBeforeApproved(closedOn, approvedOn);

        if (closedOn > today)
            throw BusinessRuleViolation.MortgageRequest.RecordLoanClosed.ClosedOnInFuture(closedOn, today);

        return new MortgageRequestLoanClosed(
            Id.Value,
            GroundLeaseId.Value,
            closedOn,
            permittedMortgageId.Value,
            [.. paidOffMortgageIds.Select(m => m.Value).Distinct()],
            recordedBy.Value);
    }

    /// <summary>
    /// Records that the homeowner dropped the loan, while it waits for a decision or after approval but before it
    /// closed. Refused without a reason, or for a withdrawn date before the request arrived or in the future.
    /// </summary>
    public MortgageRequestWithdrawn Withdraw(DateOnly withdrawnOn, string reason, DateOnly today, UserId withdrawnBy)
    {
        if (Status != MortgageRequestStatus.Pending && Status != MortgageRequestStatus.Approved)
            throw BusinessRuleViolation.MortgageRequest.Withdraw.NotOpen(Status);

        if (string.IsNullOrWhiteSpace(reason))
            throw BusinessRuleViolation.MortgageRequest.Withdraw.ReasonRequired();

        CheckDecisionDate(withdrawnOn, today,
            BusinessRuleViolation.MortgageRequest.Withdraw.WithdrawnOnBeforeReceived,
            BusinessRuleViolation.MortgageRequest.Withdraw.WithdrawnOnInFuture);

        return new MortgageRequestWithdrawn(Id.Value, GroundLeaseId.Value, withdrawnOn, reason.Trim(), withdrawnBy.Value);
    }

    private static MortgageRequestAppraisalRecorded Appraised(
        MortgageRequestId id,
        GroundLease lease,
        Money totalDebt,
        LoanAppraisal appraisal,
        Money? closingCosts,
        DateOnly today,
        UserId recordedBy)
    {
        if (appraisal.AppraisalDate > today)
            throw BusinessRuleViolation.MortgageRequest.RecordAppraisal.AppraisalDateInFuture(appraisal.AppraisalDate, today);

        if (lease.OriginalAppraisedValue is not { } originalAppraisedValue)
            throw BusinessRuleViolation.MortgageRequest.RecordAppraisal.LeaseFiguresMissing();

        var price = ResalePrice.Calculate(
            lease.ResalePolicyTerms,
            lease.BasePrice,
            originalAppraisedValue,
            lease.LeaseStartDate,
            appraisal.AppraisedValue.Amount,
            appraisal.AppraisalDate,
            approvedImprovements: null,
            appraisal.CurrentIndexValue);

        if (price.PurchaseOptionPrice <= Money.Zero)
            throw BusinessRuleViolation.MortgageRequest.RecordAppraisal.PurchaseOptionPriceNonPositive(price.PurchaseOptionPrice.Amount);

        var check = MortgageDebtCheck.Calculate(price.PurchaseOptionPrice, lease.MortgageDebtCap, totalDebt);

        return new MortgageRequestAppraisalRecorded(
            id.Value,
            appraisal.AppraisedValue,
            appraisal.AppraisalDate,
            appraisal.CurrentIndexValue,
            closingCosts,
            check.PurchaseOptionPrice,
            check.Cap.Percent,
            check.CapAmount,
            check.DebtPercent,
            recordedBy.Value);
    }

    private static void CheckClosingCosts(Money? closingCosts)
    {
        if (closingCosts is not null && closingCosts < Money.Zero)
            throw BusinessRuleViolation.MortgageRequest.ClosingCostsNegative(closingCosts.Amount);
    }

    private void CheckDecisionDate(
        DateOnly date,
        DateOnly today,
        Func<DateOnly, DateOnly, Exception> beforeReceived,
        Func<DateOnly, DateOnly, Exception> inFuture)
    {
        if (date < ReceivedOn)
            throw beforeReceived(date, ReceivedOn);

        if (date > today)
            throw inFuture(date, today);
    }

    /// <summary>Evolves a new request from its stream's first event.</summary>
    public static MortgageRequest Create(MortgageRequestReceived e)
    {
        ArgumentNullException.ThrowIfNull(e);

        return new MortgageRequest
        {
            Id = MortgageRequestId.Create(e.AggregateId),
            GroundLeaseId = GroundLeaseId.Create(e.GroundLeaseId),
            Status = MortgageRequestStatus.Pending,
            ReceivedOn = e.ReceivedOn,
            Terms = MortgageLoanTerms.Create(
                e.Lender, e.Purpose, e.Principal, e.TotalDebtAfterClosing, e.InterestRatePercent, e.RateType, e.TermYears),
            ExpectedClosingOn = e.ExpectedClosingOn,
            ClosingCosts = e.ClosingCosts,
        };
    }

    /// <summary>Applies <see cref="MortgageRequestAppraisalRecorded"/>.</summary>
    public void Apply(MortgageRequestAppraisalRecorded e)
    {
        Appraisal = LoanAppraisal.Create(e.AppraisedValue, e.AppraisalDate, e.CurrentIndexValue);
        ClosingCosts = e.ClosingCosts;
        DebtCheck = MortgageDebtCheck.Calculate(e.PurchaseOptionPrice, MortgageDebtCap.Create(e.MaxMortgageDebtPercent), Terms.TotalDebtAfterClosing);
    }

    /// <summary>Applies <see cref="MortgageRequestApproved"/>.</summary>
    public void Apply(MortgageRequestApproved e)
    {
        Status = MortgageRequestStatus.Approved;
        ApprovedOn = e.ApprovedOn;
    }

    /// <summary>Applies <see cref="MortgageRequestDenied"/>.</summary>
    public void Apply(MortgageRequestDenied _) => Status = MortgageRequestStatus.Denied;

    /// <summary>Applies <see cref="MortgageRequestLoanClosed"/>.</summary>
    public void Apply(MortgageRequestLoanClosed _) => Status = MortgageRequestStatus.Closed;

    /// <summary>Applies <see cref="MortgageRequestWithdrawn"/>.</summary>
    public void Apply(MortgageRequestWithdrawn _) => Status = MortgageRequestStatus.Withdrawn;
}
