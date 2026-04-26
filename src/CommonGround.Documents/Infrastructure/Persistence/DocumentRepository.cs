using CommonGround.Documents.Domain.Documents;
using CommonGround.SharedKernel.Persistence;
using Marten;
using Microsoft.Extensions.Logging;

namespace CommonGround.Documents.Infrastructure.Persistence;

internal sealed class DocumentRepository(
    IDocumentSession session,
    ILogger<DocumentRepository> logger)
    : MartenRepository<Document, DocumentId>(session, logger),
    IDocumentRepository
{
}
