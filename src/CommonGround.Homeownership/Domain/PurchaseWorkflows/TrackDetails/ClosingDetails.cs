namespace CommonGround.Homeownership.Domain.PurchaseWorkflows.TrackDetails;

/// <summary>Details recorded when the purchase is closed.</summary>
/// <param name="ActualClosingDate">Actual date of closing.</param>
/// <param name="Notes">Optional closing notes.</param>
public sealed record ClosingDetails(DateOnly ActualClosingDate, string? Notes);
