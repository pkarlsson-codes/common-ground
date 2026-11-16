using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeases.Events;

public sealed record GroundLeaseTerminated(
    string AggregateId,
    string Reason,
    DateOnly TerminatedDate,
    string TerminatedBy) : DomainEvent(AggregateId);
