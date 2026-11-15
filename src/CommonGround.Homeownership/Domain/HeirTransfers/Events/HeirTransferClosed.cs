using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.HeirTransfers.Events;

/// <summary>Raised when the home does not pass to the heir. The lease is unchanged.</summary>
/// <param name="AggregateId">The heir transfer ID.</param>
/// <param name="GroundLeaseId">The lease the home is held on.</param>
/// <param name="Reason">Why it closed.</param>
/// <param name="Notes">Anything staff want to note.</param>
/// <param name="ClosedBy">The staff user who closed it.</param>
public sealed record HeirTransferClosed(
    string AggregateId,
    string GroundLeaseId,
    HeirTransferCloseReason Reason,
    string? Notes,
    string ClosedBy) : DomainEvent(AggregateId);
