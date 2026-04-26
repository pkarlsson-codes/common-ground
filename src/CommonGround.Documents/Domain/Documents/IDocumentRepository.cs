using CommonGround.SharedKernel.Persistence;

namespace CommonGround.Documents.Domain.Documents;

/// <summary>Repository for <see cref="Document"/> aggregates.</summary>
public interface IDocumentRepository : IRepository<Document, DocumentId>
{
}
