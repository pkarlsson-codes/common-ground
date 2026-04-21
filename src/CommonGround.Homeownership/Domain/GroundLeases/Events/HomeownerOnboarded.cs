using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeases.Events;

/// <summary>Raised when a homeowner is first onboarded to a ground lease.</summary>
public sealed record HomeownerOnboarded(
    string AggregateId,
    string FullName,
    string Email,
    string? Phone,
    string OnboardedBy,
    DateTimeOffset OccurredAt) : IDomainEvent;
