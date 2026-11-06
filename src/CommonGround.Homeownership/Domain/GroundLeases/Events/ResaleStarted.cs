using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeases.Events;

/// <summary>Raised when a resale opens on a lease, so the lease refuses another until it closes.</summary>
/// <param name="AggregateId">The ground lease's ID.</param>
/// <param name="ResaleId">The resale that opened.</param>
public sealed record ResaleStarted(
    string AggregateId,
    string ResaleId) : DomainEvent(AggregateId);
