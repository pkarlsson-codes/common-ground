using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.Resales.Events;

/// <summary>Raised when a homeowner gives notice that they intend to sell, opening a resale.</summary>
/// <param name="AggregateId">The new resale's ID.</param>
/// <param name="GroundLeaseId">The lease of the homeowner selling.</param>
/// <param name="LandParcelId">The leased land parcel.</param>
/// <param name="HomeId">The home being sold.</param>
/// <param name="HomeownerFullName">The selling homeowner's full name.</param>
/// <param name="NoticeDate">The day the homeowner gave notice.</param>
/// <param name="Channel">How the notice reached the trust.</param>
/// <param name="Notes">Anything staff noted about the notice.</param>
/// <param name="OptionWindowEndsOn">The last day of the trust's purchase option: the notice date plus the lease's option window.</param>
/// <param name="RecordedBy">The user who recorded the notice.</param>
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
