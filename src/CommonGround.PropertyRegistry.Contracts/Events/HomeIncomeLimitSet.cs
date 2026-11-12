using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Contracts.Events;

/// <summary>
/// Integration event published when staff set a home's income limit. Consumed by HomeBuyers, whose selections
/// judge candidates' income against it.
/// </summary>
/// <param name="HomeId">The home's ID.</param>
/// <param name="PercentOfAmi">The limit as a percent of area median income, or null for no income limit.</param>
public sealed record HomeIncomeLimitSet(
    string HomeId,
    int? PercentOfAmi) : IntegrationEvent;
