using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.Households.Events;

/// <summary>A member was removed from a household.</summary>
public sealed record HouseholdMemberRemoved(
    string AggregateId,
    string MemberId,
    string RemovedBy) : DomainEvent(AggregateId);
