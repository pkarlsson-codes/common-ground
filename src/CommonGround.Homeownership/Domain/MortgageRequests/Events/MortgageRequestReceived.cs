using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.MortgageRequests.Events;

/// <summary>Raised when the trust receives a homeowner's request to mortgage the home (Model Lease Art 8.3).</summary>
/// <param name="AggregateId">The mortgage request ID.</param>
/// <param name="GroundLeaseId">The homeowner's lease.</param>
/// <param name="HomeId">The home the loan is secured on.</param>
/// <param name="HomeownerFullName">The homeowner's full name when the request arrived.</param>
/// <param name="ReceivedOn">The day the request reached the trust.</param>
/// <param name="Lender">The lender making the loan.</param>
/// <param name="Purpose">What the loan is for.</param>
/// <param name="Principal">The loan amount.</param>
/// <param name="TotalDebtAfterClosing">All mortgage debt on the home once the loan closes.</param>
/// <param name="InterestRatePercent">The loan's yearly interest rate, in percent.</param>
/// <param name="RateType">Whether the rate is fixed or adjustable.</param>
/// <param name="TermYears">The loan's term in years.</param>
/// <param name="ExpectedClosingOn">The day the loan is expected to close.</param>
/// <param name="ClosingCosts">The loan's closing costs, or null when not yet known.</param>
/// <param name="Notes">Anything staff noted about the request.</param>
/// <param name="ReceivedBy">The staff user who recorded it.</param>
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
