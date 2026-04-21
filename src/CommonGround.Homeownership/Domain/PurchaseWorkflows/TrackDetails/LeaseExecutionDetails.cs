namespace CommonGround.Homeownership.Domain.PurchaseWorkflows.TrackDetails;

/// <summary>Details recorded when the ground lease is executed (signed).</summary>
/// <param name="SignedDate">Date both parties signed the lease.</param>
public sealed record LeaseExecutionDetails(DateOnly SignedDate);
