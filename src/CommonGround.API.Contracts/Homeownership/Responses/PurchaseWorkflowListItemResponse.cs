namespace CommonGround.API.Contracts.Homeownership.Responses;

/// <summary>Summary item for purchase workflow list responses.</summary>
public sealed record PurchaseWorkflowListItemResponse(
    string Id,
    string GroundLeaseId,
    string BuyerFullName,
    decimal PurchasePrice,
    string Phase,
    string CreatedAt);
