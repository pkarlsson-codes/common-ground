namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to withdraw a purchase.</summary>
public sealed record WithdrawPurchaseRequest(
    DateOnly WithdrawnDate,
    string? Reason);
