namespace CommonGround.API.Contracts.Documents.Responses;

/// <summary>Detail response for a document.</summary>
public sealed record DocumentResponse(
    string Id,
    string Title,
    string Sensitivity,
    string Retention,
    string CurrentVersionId,
    string CurrentFileName,
    string CurrentContentType,
    long CurrentSizeBytes,
    string UploadedBy,
    DateTimeOffset UploadedAt,
    bool IsDeleted,
    DateTimeOffset? ExpiresAt,
    IReadOnlyList<string> Tags,
    IReadOnlyList<DocumentVersionResponse> Versions,
    IReadOnlyList<DocumentAttachmentResponse> Attachments);

/// <summary>Single version snapshot in a document response.</summary>
public sealed record DocumentVersionResponse(
    string VersionId,
    int VersionNumber,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Sha256,
    DateTimeOffset UploadedAt);

/// <summary>Single attachment in a document response.</summary>
public sealed record DocumentAttachmentResponse(
    string EntityType,
    string EntityId,
    string? Role,
    DateTimeOffset AttachedAt);
