using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.MortgageRequests.Events;

/// <summary>Raised when the trust receives a homeowner's request to mortgage the home (Model Lease Art 8.3).</summary>
/// <param name="HomeownerFullName">The homeowner's full name when the request arrived.</param>
/// <param name="ReceivedOn">The day the request reached the trust.</param>
/// <param name="TotalDebtAfterClosing">All mortgage debt on the home once the loan closes.</param>
/// <param name="InterestRatePercent">The loan's yearly interest rate, in percent.</param>
/// <param name="TermYears">The loan's term in years.</param>
/// <param name="ExpectedClosingOn">The day the loan is expected to close.</param>
/// <param name="ClosingCosts">The loan's closing costs, or null when not yet known.</param>
public sealed record MortgageRequestReceived(
    string AggregateId,
    string GroundLeaseId,
    string HomeId,
    string? HomeownerFullName,
    DateOnly ReceivedOn,
    string Lender,
    MortgagePurpose Purpose,
    Money Principal,
    Money TotalDebtAfterClosing,
    decimal InterestRatePercent,
    MortgageRateType RateType,
    int TermYears,
    DateOnly ExpectedClosingOn,
    Money? ClosingCosts,
    string? Notes,
    string ReceivedBy) : DomainEvent(AggregateId);
