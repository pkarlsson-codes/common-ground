using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Contracts.Events;

/// <summary>Integration event published by the Homeownership BC when a homeowner is onboarded to a ground lease.</summary>
public sealed record GroundLeaseCreated(
    string GroundLeaseId,
    string FullName,
    string Email,
    string? Phone,
    string OnboardedBy) : IntegrationEvent;
