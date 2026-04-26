using CommonGround.SharedKernel.Domain;

namespace CommonGround.Documents.Domain.Documents;

/// <summary>Strongly-typed identifier for a single version of a Document.</summary>
public sealed class DocumentVersionId(string value)
    : EntityId<DocumentVersionId>(value, Prefix)
{
    private const string Prefix = "docver";
}
