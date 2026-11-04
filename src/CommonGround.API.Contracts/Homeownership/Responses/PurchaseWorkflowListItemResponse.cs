namespace CommonGround.API.Contracts.Homeownership.Responses;

/// <summary>Summary item for purchase workflow list responses.</summary>
public sealed record PurchaseWorkflowListItemResponse(
    string Id,
    string GroundLeaseTemplateId,
    string HomeId,
    string? HomeLabel,
    string? HouseholdId,
    string BuyerFullName,
    decimal PurchasePrice,
    string Phase,
    DateTimeOffset CreatedAt,
    DateTimeOffset StepEnteredAt,
    DateOnly? ScheduledClosingDate,
    string NextStep);
