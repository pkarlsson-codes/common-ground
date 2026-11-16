using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.Resales.Events;

/// <summary>Raised when a homeowner gives notice that they intend to sell, opening a resale.</summary>
/// <param name="NoticeDate">The day the homeowner gave notice.</param>
/// <param name="OptionWindowEndsOn">The last day of the trust's purchase option: the notice date plus the lease's option window.</param>
public sealed record IntentToSellRecorded(
    string AggregateId,
    string GroundLeaseId,
    string LandParcelId,
    string HomeId,
    string? HomeownerFullName,
    DateOnly NoticeDate,
    NoticeChannel Channel,
    string? Notes,
    DateOnly OptionWindowEndsOn,
    string RecordedBy) : DomainEvent(AggregateId);
