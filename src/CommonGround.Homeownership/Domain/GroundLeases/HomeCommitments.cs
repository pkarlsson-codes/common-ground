using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeases;

/// <summary>
/// What a home is already promised to when a lease is created for it: an active lease or an open purchase.
/// A home with neither is free to lease. A household chosen for a home starts its purchase, so a chosen home
/// has an open purchase.
/// </summary>
public sealed class HomeCommitments : ValueObject
{
    private HomeCommitments(string? activeLeaseId, string? openPurchaseWorkflowId)
    {
        ActiveLeaseId = activeLeaseId;
        OpenPurchaseWorkflowId = openPurchaseWorkflowId;
    }

    /// <summary>Gets commitments for a home that has none.</summary>
    public static HomeCommitments None { get; } = new(null, null);

    /// <summary>Creates the commitments found on a home.</summary>
    /// <param name="activeLeaseId">The home's active ground lease, if any.</param>
    /// <param name="openPurchaseWorkflowId">The home's purchase workflow that is still open, if any.</param>
    public static HomeCommitments Create(string? activeLeaseId, string? openPurchaseWorkflowId) =>
        new(activeLeaseId, openPurchaseWorkflowId);

    /// <summary>Gets the home's active ground lease, if any.</summary>
    public string? ActiveLeaseId { get; }

    /// <summary>Gets the home's purchase workflow that is still open, if any.</summary>
    public string? OpenPurchaseWorkflowId { get; }

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return ActiveLeaseId ?? string.Empty;
        yield return OpenPurchaseWorkflowId ?? string.Empty;
    }
}
