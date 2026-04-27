namespace CommonGround.API.Contracts.Documents.Requests;

/// <summary>
/// JSON metadata part of an add-version multipart upload. The file part is named "file";
/// this metadata part is named "metadata" and is parsed as JSON.
/// </summary>
public sealed record AddDocumentVersionMetadata(string? Note = null);
