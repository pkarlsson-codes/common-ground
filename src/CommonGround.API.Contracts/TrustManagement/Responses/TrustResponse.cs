namespace CommonGround.API.Contracts.TrustManagement.Responses;

/// <summary>A trust.</summary>
/// <param name="Id">The trust identifier.</param>
/// <param name="Name">The name of the trust.</param>
/// <param name="TimeZone">The IANA time zone id the trust operates in; "today" follows it.</param>
public sealed record TrustResponse(string Id, string Name, string TimeZone);
