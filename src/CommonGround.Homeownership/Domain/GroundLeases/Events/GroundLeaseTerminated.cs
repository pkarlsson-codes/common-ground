using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeases.Events;

/// <summary>Raised when a ground lease is terminated.</summary>
/// <param name="AggregateId">The ground lease identifier.</param>
/// <param name="Reason">The reason for termination.</param>
/// <param name="TerminatedDate">The date the lease was terminated.</param>
/// <param name="TerminatedBy">The user who terminated the lease.</param>
public sealed record GroundLeaseTerminated(
    string AggregateId,
    string Reason,
    DateOnly TerminatedDate,
    string TerminatedBy) : DomainEvent(AggregateId);
