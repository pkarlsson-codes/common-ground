using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeases.Events;

/// <summary>Raised when a lease's open resale closes, so the lease can take another.</summary>
/// <param name="AggregateId">The ground lease's ID.</param>
/// <param name="ResaleId">The resale that closed.</param>
public sealed record ResaleClosed(
    string AggregateId,
    string ResaleId) : DomainEvent(AggregateId);
