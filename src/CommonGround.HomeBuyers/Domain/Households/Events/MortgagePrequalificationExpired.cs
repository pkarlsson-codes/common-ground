using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.Households.Events;

public sealed record MortgagePrequalificationExpired(
    string AggregateId,
    DateOnly OriginalExpiryDate) : DomainEvent(AggregateId);