using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.HomeownerApplications.Events;

/// <summary>
/// Staff confirmed that an approved household still qualifies on its changed figures, clearing the application's
/// mark. <paramref name="AnnualIncome"/> and <paramref name="HouseholdSize"/> are the figures confirmed.
/// </summary>
public sealed record ApplicationQualificationConfirmed(
    string AggregateId,
    string HouseholdId,
    DateOnly ConfirmedDate,
    Money AnnualIncome,
    int HouseholdSize,
    string ConfirmedBy) : DomainEvent(AggregateId);
