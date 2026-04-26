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
    IReadOnlyList<DocumentAttachmentResponse> Attachments);

/// <summary>Single attachment in a document response.</summary>
public sealed record DocumentAttachmentResponse(
    string EntityType,
    Guid EntityId,
    string? Role,
    DateTimeOffset AttachedAt);
