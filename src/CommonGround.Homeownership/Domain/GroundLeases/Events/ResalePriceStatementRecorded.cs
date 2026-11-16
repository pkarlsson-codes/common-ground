using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeases.Events;

/// <summary>
/// Raised when staff record that a resale price statement was sent to the homeowner (Ch 23, Disclosure). A record of
/// a letter sent, not a price the lease is bound to.
/// </summary>
/// <param name="StatementDate">The letter's date, the day the price was worked out on, and the day it was sent.</param>
/// <param name="YearsOwned">Full years from the lease start to the statement date.</param>
/// <param name="MarketValue">The estimated market value used; null unless the formula is appraisal based.</param>
/// <param name="MarketValueSource">Where the market value came from; null unless the formula is appraisal based.</param>
/// <param name="IndexValue">The index value used; null unless the formula is index based.</param>
/// <param name="ImprovementsCredit">The improvements credit counted, after the cap; null for none.</param>
/// <param name="Price">The most the home may sell for on the statement date under the formula.</param>
public sealed record ResalePriceStatementRecorded(
    string AggregateId,
    DateOnly StatementDate,
    NoticeDelivery Delivery,
    int YearsOwned,
    Money? MarketValue,
    string? MarketValueSource,
    decimal? IndexValue,
    Money? ImprovementsCredit,
    Money Price,
    string RecordedBy) : DomainEvent(AggregateId);
