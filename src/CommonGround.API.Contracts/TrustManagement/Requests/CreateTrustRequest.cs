namespace CommonGround.API.Contracts.TrustManagement.Requests;

/// <summary>Request to create a new trust.</summary>
/// <param name="Name">The name of the trust.</param>
/// <param name="TimeZone">The IANA time zone id the trust operates in, e.g. "America/Denver".</param>
public sealed record CreateTrustRequest(string Name, string TimeZone);
