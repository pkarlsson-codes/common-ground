using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.Households.Events;

public sealed record HouseholdMemberRemoved(
    string AggregateId,
    string MemberId,
    string RemovedBy) : DomainEvent(AggregateId);
