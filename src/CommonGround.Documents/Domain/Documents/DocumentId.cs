using CommonGround.SharedKernel.Domain;

namespace CommonGround.Documents.Domain.Documents;

/// <summary>Strongly-typed identifier for a Document.</summary>
public sealed class DocumentId(string value)
    : EntityId<DocumentId>(value, Prefix)
{
    private const string Prefix = "doc";
}
