using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.Resales.Events;

/// <summary>Raised when staff print a payoff statement for the closing agent. A record of a letter sent, not a figure anyone is bound to.</summary>
/// <param name="AggregateId">The resale's ID.</param>
/// <param name="GroundLeaseId">The seller's lease the figures come from.</param>
/// <param name="GoodThrough">The day the figures were worked out for.</param>
/// <param name="TotalOwed">The amount the letter asked for.</param>
/// <param name="RecordedBy">The staff user who printed it.</param>
public sealed record PayoffStatementRecorded(
    string AggregateId,
    string GroundLeaseId,
    DateOnly GoodThrough,
    Money TotalOwed,
    string RecordedBy) : DomainEvent(AggregateId);
