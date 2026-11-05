using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeases;

/// <summary>
/// What a home is already promised to when a lease is created for it: an active lease, an open purchase,
/// or an open home selection. A home with none of them is free to lease.
/// </summary>
public sealed class HomeCommitments : ValueObject
{
    private HomeCommitments(string? activeLeaseId, string? openPurchaseWorkflowId, string? openHomeSelectionId)
    {
        ActiveLeaseId = activeLeaseId;
        OpenPurchaseWorkflowId = openPurchaseWorkflowId;
        OpenHomeSelectionId = openHomeSelectionId;
    }

    /// <summary>Gets commitments for a home that has none.</summary>
    public static HomeCommitments None { get; } = new(null, null, null);

    /// <summary>Creates the commitments found on a home.</summary>
    /// <param name="activeLeaseId">The home's active ground lease, if any.</param>
    /// <param name="openPurchaseWorkflowId">The home's purchase workflow that is still open, if any.</param>
    /// <param name="openHomeSelectionId">The home's selection that is still choosing a household, if any.</param>
    public static HomeCommitments Create(string? activeLeaseId, string? openPurchaseWorkflowId, string? openHomeSelectionId) =>
        new(activeLeaseId, openPurchaseWorkflowId, openHomeSelectionId);

    /// <summary>Gets the home's active ground lease, if any.</summary>
    public string? ActiveLeaseId { get; }

    /// <summary>Gets the home's purchase workflow that is still open, if any.</summary>
    public string? OpenPurchaseWorkflowId { get; }

    /// <summary>Gets the home's selection that is still choosing a household, if any.</summary>
    public string? OpenHomeSelectionId { get; }

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return ActiveLeaseId ?? string.Empty;
        yield return OpenPurchaseWorkflowId ?? string.Empty;
        yield return OpenHomeSelectionId ?? string.Empty;
    }
}
