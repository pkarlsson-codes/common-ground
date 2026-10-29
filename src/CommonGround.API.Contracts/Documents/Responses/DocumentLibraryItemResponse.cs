namespace CommonGround.API.Contracts.Documents.Responses;

/// <summary>One row in the document library listing response.</summary>
public sealed record DocumentLibraryItemResponse(
    string Id,
    string Title,
    string Sensitivity,
    string Retention,
    string CurrentFileName,
    string CurrentContentType,
    long CurrentSizeBytes,
    int CurrentVersionNumber,
    string UploadedBy,
    DateTimeOffset UploadedAt,
    DateTimeOffset LastModifiedAt,
    bool IsDeleted,
    DateOnly? ExpiresOn,
    IReadOnlyList<string> Tags);
