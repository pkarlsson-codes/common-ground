using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.Households.Events;

public sealed record HouseholdRegistered(
    string AggregateId,
    IReadOnlyList<RegisteredMember> Members,
    decimal AnnualIncome,
    bool RequiresAccessibility,
    string RegisteredById)
    : DomainEvent(AggregateId);

public sealed record RegisteredMember(
    string Name,
    string Email,
    string Phone,
    bool IsPrimary);