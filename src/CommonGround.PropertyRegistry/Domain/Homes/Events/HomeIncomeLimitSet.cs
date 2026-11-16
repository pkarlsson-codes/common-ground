using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.Homes.Events;

/// <param name="PercentOfAmi">The limit as a percent of area median income, or null for no income limit.</param>
public sealed record HomeIncomeLimitSet(
    string AggregateId,
    int? PercentOfAmi,
    string SetBy) : DomainEvent(AggregateId);
