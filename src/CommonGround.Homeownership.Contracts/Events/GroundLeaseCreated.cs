using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Contracts.Events;

/// <summary>
/// Integration event published by the Homeownership BC when a ground lease is created, from a purchase closing
/// or directly. Consumed by Stewardship to open the lease's case, and by PropertyRegistry to mark the home leased.
/// </summary>
public sealed record GroundLeaseCreated(
    string GroundLeaseId,
    string HomeId,
    DateOnly LeaseStartDate,
    string FullName,
    string Email,
    string? Phone,
    string OnboardedBy) : IntegrationEvent;
