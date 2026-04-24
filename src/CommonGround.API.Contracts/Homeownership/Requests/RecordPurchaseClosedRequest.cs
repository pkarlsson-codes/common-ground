namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to record a purchase closed.</summary>
public sealed record RecordPurchaseClosedRequest(
    string ActualClosingDate,
    string? Notes);
