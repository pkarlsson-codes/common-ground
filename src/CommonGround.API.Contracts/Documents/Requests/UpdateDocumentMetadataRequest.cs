namespace CommonGround.API.Contracts.Documents.Requests;

/// <summary>Request body for editing a document's metadata. Null fields leave existing values untouched.</summary>
/// <param name="Title">New title, or null to leave unchanged.</param>
/// <param name="Sensitivity">New sensitivity display name, or null to leave unchanged.</param>
/// <param name="ExpiresOn">New expiry date, or null to leave unchanged (combined with <paramref name="ClearExpiresOn"/>).</param>
/// <param name="ClearExpiresOn">When true, explicitly clears the expiry instead of leaving it.</param>
public sealed record UpdateDocumentMetadataRequest(
    string? Title = null,
    string? Sensitivity = null,
    DateOnly? ExpiresOn = null,
    bool ClearExpiresOn = false);
