using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.Households.Events;

public sealed record HouseholdCircumstancesUpdated(
    string AggregateId,
    Money AnnualIncome,
    bool RequiresAccessibility,
    string UpdatedBy) : DomainEvent(AggregateId);
