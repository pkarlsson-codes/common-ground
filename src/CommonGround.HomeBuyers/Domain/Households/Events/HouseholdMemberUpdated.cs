using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.Households.Events;

/// <summary>A household member's contact details were updated.</summary>
public sealed record HouseholdMemberUpdated(
    string AggregateId,
    string MemberId,
    string Name,
    string Email,
    string Phone,
    string UpdatedBy) : DomainEvent(AggregateId);
