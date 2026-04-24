namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to record title cleared.</summary>
public sealed record RecordTitleClearedRequest(
    string ClearedDate,
    string? TitleCompanyName);
