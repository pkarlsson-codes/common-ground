using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.MortgageRequests.Events;

/// <summary>
/// Raised when the trust gives written permission for a requested loan (Model Lease Art 8.1). Keeps the debt check it
/// decided on, so an auditor can see why a loan above the cap was permitted.
/// </summary>
/// <param name="ApprovedOn">The day the trust approved it.</param>
/// <param name="MaxMortgageDebtPercent">The lease's debt cap percent.</param>
/// <param name="DebtCapAmount">The most total debt may be.</param>
/// <param name="TotalDebtAfterClosing">All mortgage debt on the home once the loan closes.</param>
/// <param name="DebtPercent">The total debt as a percent of the purchase option price.</param>
/// <param name="OverCap">Whether the total debt is above the cap amount.</param>
/// <param name="Reason">Why the trust approved it; required when over the cap.</param>
public sealed record MortgageRequestApproved(
    string AggregateId,
    string GroundLeaseId,
    DateOnly ApprovedOn,
    Money PurchaseOptionPrice,
    int MaxMortgageDebtPercent,
    Money DebtCapAmount,
    Money TotalDebtAfterClosing,
    decimal DebtPercent,
    bool OverCap,
    string? Reason,
    string ApprovedBy) : DomainEvent(AggregateId);
