namespace CommonGround.API.Contracts.Documents.Requests;

/// <summary>Soft-deletes a document with a reason.</summary>
public sealed record SoftDeleteDocumentRequest(string Reason);
