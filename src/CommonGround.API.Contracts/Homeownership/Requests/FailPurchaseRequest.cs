namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to fail a purchase.</summary>
public sealed record FailPurchaseRequest(
    string FailureReason,
    DateOnly FailedDate);
