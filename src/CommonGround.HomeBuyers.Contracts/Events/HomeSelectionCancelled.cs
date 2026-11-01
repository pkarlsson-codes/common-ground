using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Contracts.Events;

/// <summary>
/// Integration event published when a home selection is cancelled without choosing anyone.
/// Consumed by PropertyRegistry to make the home available again.
/// </summary>
public sealed record HomeSelectionCancelled(
    string HomeSelectionId,
    string HomeId) : IntegrationEvent;
