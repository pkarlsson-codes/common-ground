using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Contracts.Events;

/// <summary>
/// Integration event published by the Homeownership BC when the trust closes on a home it bought back from its
/// homeowner. Consumed by PropertyRegistry to show the home vacant again, either offered to the waiting list or
/// held by the trust.
/// </summary>
public sealed record ResaleBoughtBack(
    string ResaleId,
    string GroundLeaseId,
    string HomeId,
    DateOnly ClosingDate,
    bool OfferedForSale,
    string? HoldReason) : IntegrationEvent;
