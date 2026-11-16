using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.HeirTransfers.Events;

/// <summary>Raised when the home does not pass to the heir. The lease is unchanged.</summary>
public sealed record HeirTransferClosed(
    string AggregateId,
    string GroundLeaseId,
    HeirTransferCloseReason Reason,
    string? Notes,
    string ClosedBy) : DomainEvent(AggregateId);
