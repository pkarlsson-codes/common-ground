namespace CommonGround.API.Contracts.Documents.Requests;

/// <summary>Attaches an existing document to an entity in another BC.</summary>
public sealed record AttachDocumentRequest(string EntityType, Guid EntityId, string? Role);
