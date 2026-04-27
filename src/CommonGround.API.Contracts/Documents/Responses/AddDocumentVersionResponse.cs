namespace CommonGround.API.Contracts.Documents.Responses;

/// <summary>Response from a successful add-version request.</summary>
public sealed record AddDocumentVersionResponse(string DocumentId, string VersionId);
