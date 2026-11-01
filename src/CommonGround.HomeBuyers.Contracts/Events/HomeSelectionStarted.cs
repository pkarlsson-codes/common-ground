using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Contracts.Events;

/// <summary>
/// Integration event published when a home selection starts choosing a household for a home.
/// Consumed by PropertyRegistry to move the home into selection.
/// </summary>
public sealed record HomeSelectionStarted(
    string HomeSelectionId,
    string HomeId,
    DateTimeOffset StartedAt) : IntegrationEvent;
