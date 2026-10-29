namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to stall a purchase workflow.</summary>
public sealed record StallWorkflowRequest(
    string StalledReason,
    DateOnly StalledDate,
    DateOnly ResolutionTargetDate);
