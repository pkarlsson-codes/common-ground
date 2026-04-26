namespace CommonGround.API.Contracts.Documents.Responses;

/// <summary>An item in the per-entity document listing.</summary>
public sealed record EntityDocumentItemResponse(
    string DocumentId,
    string Title,
    string CurrentFileName,
    string CurrentContentType,
    long CurrentSizeBytes,
    string? Role,
    DateTimeOffset AttachedAt);
