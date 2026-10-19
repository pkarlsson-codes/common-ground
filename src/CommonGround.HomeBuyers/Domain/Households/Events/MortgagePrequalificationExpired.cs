using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.Households.Events;

/// <summary>Mortgage prequalification expired event.</summary>
public sealed record MortgagePrequalificationExpired(
    string AggregateId,
    DateOnly OriginalExpiryDate) : DomainEvent(AggregateId);