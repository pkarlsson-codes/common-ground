using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.MortgageRequests.Events;

/// <summary>
/// Raised when the loan's appraisal is recorded on a mortgage request, with the debt check it gives. A later
/// appraisal replaces it.
/// </summary>
/// <param name="AggregateId">The mortgage request ID.</param>
/// <param name="AppraisedValue">The home's appraised value for the loan.</param>
/// <param name="AppraisalDate">The day of the appraisal.</param>
/// <param name="CurrentIndexValue">The index value on that day, when the lease's formula is index based.</param>
/// <param name="ClosingCosts">The loan's closing costs as now known, or null when not yet known.</param>
/// <param name="PurchaseOptionPrice">The purchase option price the appraisal gives under the lease's formula.</param>
/// <param name="MaxMortgageDebtPercent">The lease's debt cap percent.</param>
/// <param name="DebtCapAmount">The most total debt may be: the cap percent of the purchase option price.</param>
/// <param name="DebtPercent">The total debt after closing as a percent of the purchase option price.</param>
/// <param name="RecordedBy">The staff user who recorded it.</param>
public sealed record MortgageRequestAppraisalRecorded(
    string AggregateId,
    Money AppraisedValue,
    DateOnly AppraisalDate,
    decimal? CurrentIndexValue,
    Money? ClosingCosts,
    Money PurchaseOptionPrice,
    int MaxMortgageDebtPercent,
    Money DebtCapAmount,
    decimal DebtPercent,
    string RecordedBy) : DomainEvent(AggregateId);
