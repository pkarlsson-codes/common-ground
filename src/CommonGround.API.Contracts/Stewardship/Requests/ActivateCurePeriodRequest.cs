namespace CommonGround.API.Contracts.Stewardship.Requests;

/// <summary>Request to activate an LRA cure period on a foreclosure prevention case.</summary>
public sealed record ActivateCurePeriodRequest(
    string LraId,
    DateOnly ActivationDate,
    int CurePeriodDays);
