using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.Households.Events;

/// <summary>A household's circumstances were updated.</summary>
public sealed record HouseholdCircumstancesUpdated(
    string AggregateId,
    decimal AnnualIncome,
    bool RequiresAccessibility,
    string UpdatedBy) : DomainEvent(AggregateId);
