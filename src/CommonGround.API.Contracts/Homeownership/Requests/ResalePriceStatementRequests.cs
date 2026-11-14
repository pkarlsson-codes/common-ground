namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request for a lease's resale price statement, without recording it.</summary>
/// <param name="StatementDate">The day to work the price out on; not in the future or before the lease start.</param>
/// <param name="MarketValue">Estimated market value; required for an appraisal-based lease, ignored otherwise.</param>
/// <param name="MarketValueSource">Where the market value came from; required with it.</param>
/// <param name="IndexValue">Index value on the statement date; required for an index-based lease, ignored otherwise.</param>
/// <param name="ImprovementsCredit">Approved improvements; counted up to the lease's improvements cap, ignored without one.</param>
public sealed record PreviewResalePriceStatementRequest(
    DateOnly StatementDate,
    decimal? MarketValue,
    string? MarketValueSource,
    decimal? IndexValue,
    decimal? ImprovementsCredit);

/// <summary>Request to record that a resale price statement was sent.</summary>
/// <param name="StatementDate">The letter's date, the day the price is worked out on and the day it was sent.</param>
/// <param name="MarketValue">Estimated market value; required for an appraisal-based lease, ignored otherwise.</param>
/// <param name="MarketValueSource">Where the market value came from; required with it.</param>
/// <param name="IndexValue">Index value on the statement date; required for an index-based lease, ignored otherwise.</param>
/// <param name="ImprovementsCredit">Approved improvements; counted up to the lease's improvements cap, ignored without one.</param>
/// <param name="Delivery">How it was sent: FirstClassMail, CertifiedMail, RegisteredMail, HandDelivered or Email.</param>
public sealed record RecordResalePriceStatementRequest(
    DateOnly StatementDate,
    decimal? MarketValue,
    string? MarketValueSource,
    decimal? IndexValue,
    decimal? ImprovementsCredit,
    string Delivery);

/// <summary>Request for the resale price statements of a yearly round, without recording them.</summary>
/// <param name="StatementDate">The day to work every price out on; not in the future.</param>
/// <param name="Leases">The leases and their figures; at most 1000.</param>
public sealed record PreviewResalePriceStatementsRequest(
    DateOnly StatementDate,
    IReadOnlyList<ResalePriceStatementLeaseFiguresRequest> Leases);

/// <summary>One lease in a round preview, with the figures its formula needs. A round has no improvements credit.</summary>
/// <param name="GroundLeaseId">The lease.</param>
/// <param name="MarketValue">Estimated market value; required for an appraisal-based lease, ignored otherwise.</param>
/// <param name="MarketValueSource">Where the market value came from; required with it.</param>
/// <param name="IndexValue">Index value on the statement date; required for an index-based lease, ignored otherwise.</param>
public sealed record ResalePriceStatementLeaseFiguresRequest(
    string GroundLeaseId,
    decimal? MarketValue,
    string? MarketValueSource,
    decimal? IndexValue);
