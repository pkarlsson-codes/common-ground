using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeases.Events;

/// <summary>Raised when a lease's open resale closes, so the lease can take another.</summary>
public sealed record ResaleClosed(
    string AggregateId,
    string ResaleId) : DomainEvent(AggregateId);
