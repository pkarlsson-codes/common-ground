namespace CommonGround.API.Contracts.Documents.Requests;

/// <summary>Request body for editing a document's metadata. Null fields leave existing values untouched.</summary>
/// <param name="Title">New title, or null to leave unchanged.</param>
/// <param name="Sensitivity">New sensitivity display name, or null to leave unchanged.</param>
/// <param name="ExpiresAt">New expiry timestamp, or null to leave unchanged (combined with <paramref name="ClearExpiresAt"/>).</param>
/// <param name="ClearExpiresAt">When true, explicitly clears the expiry instead of leaving it.</param>
public sealed record UpdateDocumentMetadataRequest(
    string? Title = null,
    string? Sensitivity = null,
    DateTimeOffset? ExpiresAt = null,
    bool ClearExpiresAt = false);
