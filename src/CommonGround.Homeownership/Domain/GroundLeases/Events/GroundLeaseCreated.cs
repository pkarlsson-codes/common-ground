using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeases.Events;

/// <summary>Raised when a ground lease is created directly, rather than from a purchase closing.</summary>
/// <param name="AggregateId">The ID of the new ground lease.</param>
/// <param name="LandParcelId">The land parcel being leased.</param>
/// <param name="HomeId">The home on the parcel.</param>
/// <param name="LeaseStartDate">When the lease starts.</param>
/// <param name="TermYears">Lease duration in years.</param>
/// <param name="CovenantTerms">Free-text covenant terms.</param>
/// <param name="FormulaKind">Which resale formula applies.</param>
/// <param name="AppreciationShare">Appreciation share (AppraisalBased only).</param>
/// <param name="IndexType">Index identifier (IndexBased only).</param>
/// <param name="BaselineIndexValue">Baseline index value (IndexBased only).</param>
/// <param name="AnnualPercentage">Annual growth rate (FixedPercentage only).</param>
/// <param name="ImprovementsCap">Maximum improvements credit, if any.</param>
/// <param name="RegisteredBy">The user who created this lease.</param>
/// <param name="HomeownerFullName">The homeowner's full name.</param>
/// <param name="HomeownerEmail">The homeowner's email.</param>
/// <param name="HomeownerPhone">The homeowner's phone number, if given.</param>
public sealed record GroundLeaseCreated(
    string AggregateId,
    string LandParcelId,
    string HomeId,
    DateOnly LeaseStartDate,
    int TermYears,
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
    string? HomeownerPhone) : DomainEvent(AggregateId);
