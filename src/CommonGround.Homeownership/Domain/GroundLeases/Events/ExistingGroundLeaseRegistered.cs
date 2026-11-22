using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeases.Events;

/// <summary>Raised when a ground lease is created directly, rather than from a purchase closing.</summary>
/// <param name="TermYears">Lease duration in years.</param>
/// <param name="OptionWindowDays">Days the trust has to buy the home after its homeowner gives notice to sell.</param>
/// <param name="LateInterestRate">Yearly interest rate on a late fee, in percent; zero charges none.</param>
/// <param name="InterestWaiverDays">Days after the due date within which a payment owes no interest.</param>
/// <param name="MaxMortgageDebtPercent">The debt cap, a whole percent of the purchase option price.</param>
/// <param name="FeesTrackedFrom">The first day the trust tracks the lease's fees in CommonGround.</param>
/// <param name="OpeningBalance">What the homeowner owed on that day.</param>
/// <param name="AppreciationShare">Appreciation share (AppraisalBased only).</param>
/// <param name="IndexType">Index identifier (IndexBased only).</param>
/// <param name="BaselineIndexValue">Baseline index value (IndexBased only).</param>
/// <param name="AnnualPercentage">Annual growth rate (FixedPercentage only).</param>
/// <param name="ImprovementsCap">Maximum improvements credit, if any.</param>
/// <param name="HomeownerPhone">The homeowner's phone number, if given.</param>
/// <param name="RequiredReplacementValue">The replacement value the home must be insured for (Art 9.4), or null when not checked.</param>
/// <param name="RequiredLiabilityPerOccurrence">The liability coverage per occurrence required, or null when not checked.</param>
public sealed record ExistingGroundLeaseRegistered(
    string AggregateId,
    string LandParcelId,
    string HomeId,
    DateOnly LeaseStartDate,
    int TermYears,
    int OptionWindowDays,
    Money LandUseFee,
    Money RepairReserveFee,
    decimal LateInterestRate,
    int InterestWaiverDays,
    int MaxMortgageDebtPercent,
    DateOnly FeesTrackedFrom,
    Money OpeningBalance,
    Money BasePrice,
    Money OriginalAppraisedValue,
    string CovenantTerms,
    FormulaKind FormulaKind,
    decimal? AppreciationShare,
    string? IndexType,
    decimal? BaselineIndexValue,
    decimal? AnnualPercentage,
    decimal? ImprovementsCap,
    string RegisteredBy,
    string HomeownerFullName,
    string HomeownerEmail,
    string? HomeownerPhone,
    Money? RequiredReplacementValue,
    Money? RequiredLiabilityPerOccurrence) : DomainEvent(AggregateId);
