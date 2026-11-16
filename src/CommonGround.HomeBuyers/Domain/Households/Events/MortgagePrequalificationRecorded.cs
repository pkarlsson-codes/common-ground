using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.Households.Events;

public sealed record MortgagePrequalificationRecorded(
    string AggregateId,
    string LenderName,
    decimal ApprovedAmount,
    DateOnly ExpiryDate,
    string RecordedBy) : DomainEvent(AggregateId);
