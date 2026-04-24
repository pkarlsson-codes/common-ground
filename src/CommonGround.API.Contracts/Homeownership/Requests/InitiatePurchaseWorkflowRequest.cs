namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to initiate a new purchase workflow.</summary>
public sealed record InitiatePurchaseWorkflowRequest(
    string GroundLeaseId,
    string BuyerFullName,
    string BuyerEmail,
    string? BuyerPhone,
    decimal PurchasePrice);
