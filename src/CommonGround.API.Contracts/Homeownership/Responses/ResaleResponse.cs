namespace CommonGround.API.Contracts.Homeownership.Responses;

/// <summary>A resale, from the homeowner's notice of intent to sell.</summary>
/// <param name="Id">The resale ID.</param>
/// <param name="GroundLeaseId">The lease of the homeowner selling.</param>
/// <param name="LandParcelId">The leased land parcel.</param>
/// <param name="HomeId">The home being sold.</param>
/// <param name="HomeLabel">The label staff know the home by, once known.</param>
/// <param name="HomeownerFullName">The selling homeowner's full name.</param>
/// <param name="Status">Open or Withdrawn.</param>
/// <param name="NoticeDate">The day the homeowner gave notice.</param>
/// <param name="NoticeChannel">How the notice reached the trust.</param>
/// <param name="Notes">Anything staff noted about the notice.</param>
/// <param name="OptionWindowEndsOn">The last day of the trust's purchase option.</param>
/// <param name="WithdrawnDate">The day the homeowner withdrew, if they did.</param>
/// <param name="WithdrawnReason">Why the homeowner withdrew, if they did.</param>
/// <param name="RecordedAt">When the notice was recorded.</param>
public sealed record ResaleResponse(
    string Id,
    string GroundLeaseId,
    string LandParcelId,
    string HomeId,
    string? HomeLabel,
    string? HomeownerFullName,
    string Status,
    DateOnly NoticeDate,
    string NoticeChannel,
    string? Notes,
    DateOnly OptionWindowEndsOn,
    DateOnly? WithdrawnDate,
    string? WithdrawnReason,
    DateTimeOffset RecordedAt);
