using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.HomeownerApplications.Events;

/// <summary>
/// The application was a candidate in home selection <paramref name="SelectionId"/> for
/// <paramref name="HomeId"/> and another household was chosen. The application stays approved.
/// <paramref name="HouseholdId"/> lets the household's selection history be built across its applications.
/// </summary>
public sealed record ApplicationPassedOver(
    string AggregateId,
    string HouseholdId,
    string SelectionId,
    string HomeId) : DomainEvent(AggregateId);
