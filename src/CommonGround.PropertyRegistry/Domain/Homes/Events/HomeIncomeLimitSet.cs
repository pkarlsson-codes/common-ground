using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.Homes.Events;

/// <summary>Raised when staff set a home's income limit.</summary>
/// <param name="AggregateId">The home's ID.</param>
/// <param name="PercentOfAmi">The limit as a percent of area median income, or null for no income limit.</param>
/// <param name="SetBy">The staff user who set the limit.</param>
public sealed record HomeIncomeLimitSet(
    string AggregateId,
    int? PercentOfAmi,
    string SetBy) : DomainEvent(AggregateId);
