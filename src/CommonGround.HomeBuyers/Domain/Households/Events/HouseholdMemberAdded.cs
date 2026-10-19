using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.Households.Events;

/// <summary>A non-primary member was added to a household.</summary>
public sealed record HouseholdMemberAdded(
    string AggregateId,
    string MemberId,
    string Name,
    string Email,
    string Phone,
    string AddedBy) : DomainEvent(AggregateId);
