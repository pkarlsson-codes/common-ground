namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to record title cleared.</summary>
public sealed record RecordTitleClearedRequest(
    DateOnly ClearedDate,
    string? TitleCompanyName);
