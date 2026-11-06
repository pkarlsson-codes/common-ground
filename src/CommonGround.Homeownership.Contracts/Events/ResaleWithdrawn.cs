using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Contracts.Events;

/// <summary>
/// Integration event published by the Homeownership BC when a homeowner withdraws their notice to sell.
/// Consumed by PropertyRegistry to show the home leased again.
/// </summary>
public sealed record ResaleWithdrawn(
    string ResaleId,
    string GroundLeaseId,
    string HomeId,
    DateOnly WithdrawnDate) : IntegrationEvent;
