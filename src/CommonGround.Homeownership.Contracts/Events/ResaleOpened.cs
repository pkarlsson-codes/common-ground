using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Contracts.Events;

/// <summary>
/// Integration event published by the Homeownership BC when a homeowner gives notice to sell and a resale opens
/// on their lease. Consumed by PropertyRegistry to show the home in resale.
/// </summary>
public sealed record ResaleOpened(
    string ResaleId,
    string GroundLeaseId,
    string HomeId,
    DateOnly NoticeDate,
    DateOnly OptionWindowEndsOn) : IntegrationEvent;
