namespace CommonGround.API.Contracts.Documents.Requests;

/// <summary>
/// JSON metadata part of a multipart upload request. The file part is named "file";
/// this metadata part is named "metadata" and is parsed as JSON.
/// </summary>
public sealed record UploadDocumentMetadata(
    string Title,
    string Sensitivity,
    string Retention,
    InitialAttachment? InitialAttachment = null);

/// <summary>Optional initial attachment carried in the upload metadata part.</summary>
public sealed record InitialAttachment(string EntityType, string EntityId, string? Role);
