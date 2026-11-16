using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.HeirTransfers.Events;

/// <summary>Raised when the trust hears of a homeowner's death and opens a transfer of the home (Model Lease Art 10.3).</summary>
/// <param name="DeceasedHomeownerFullName">The lease's homeowner when the transfer opened.</param>
/// <param name="DiedOn">The day the homeowner died.</param>
/// <param name="NoticeReceivedOn">The day the executor told the trust.</param>
/// <param name="NotifiedByContact">How to reach them, if given.</param>
public sealed record HeirTransferOpened(
    string AggregateId,
    string GroundLeaseId,
    string HomeId,
    string? DeceasedHomeownerFullName,
    DateOnly DiedOn,
    DateOnly NoticeReceivedOn,
    string NotifiedBy,
    string? NotifiedByContact,
    string? Notes,
    string OpenedBy) : DomainEvent(AggregateId);
