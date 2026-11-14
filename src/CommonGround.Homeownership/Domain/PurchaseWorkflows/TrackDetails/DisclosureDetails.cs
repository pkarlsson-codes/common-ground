namespace CommonGround.Homeownership.Domain.PurchaseWorkflows.TrackDetails;

/// <summary>Details recorded when the buyer's signed disclosure acknowledgment is recorded.</summary>
/// <param name="AcknowledgedDate">The date the buyer signed.</param>
/// <param name="Notes">Optional notes.</param>
/// <param name="DocumentId">The signed acknowledgment in Documents.</param>
public sealed record DisclosureDetails(DateOnly AcknowledgedDate, string? Notes, string DocumentId);
