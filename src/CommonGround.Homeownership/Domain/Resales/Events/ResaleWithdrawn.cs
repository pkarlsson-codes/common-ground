using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.Resales.Events;

/// <param name="WithdrawnDate">The day the homeowner withdrew.</param>
public sealed record ResaleWithdrawn(
    string AggregateId,
    string GroundLeaseId,
    string HomeId,
    DateOnly WithdrawnDate,
    string Reason,
    string WithdrawnBy) : DomainEvent(AggregateId);
