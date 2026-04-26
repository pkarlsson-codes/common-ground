using CommonGround.API.Contracts.Documents.Requests;
using CommonGround.API.Contracts.Documents.Responses;
using CommonGround.SharedKernel.Infrastructure.API;

namespace CommonGround.API.Clients;

/// <summary>Typed HTTP client for the Documents API endpoints.</summary>
public interface IDocumentsApiClient
{
    /// <summary>Uploads a new document — multipart/form-data with metadata + file parts.</summary>
    /// <param name="metadata">The document metadata.</param>
    /// <param name="content">The file stream content.</param>
    /// <param name="fileName">The name of the file being uploaded.</param>
    /// <param name="contentType">The MIME type of the file.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The upload response containing the document ID.</returns>
    Task<UploadDocumentResponse> UploadDocumentAsync(
        UploadDocumentMetadata metadata,
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken);

    /// <summary>Gets a document by ID.</summary>
    /// <param name="documentId">The ID of the document to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The document response.</returns>
    Task<DocumentResponse> GetDocumentAsync(string documentId, CancellationToken cancellationToken);

    /// <summary>Attaches a document to an entity.</summary>
    /// <param name="documentId">The ID of the document to attach.</param>
    /// <param name="request">The attachment request details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AttachDocumentAsync(string documentId, AttachDocumentRequest request, CancellationToken cancellationToken);

    /// <summary>Soft-deletes a document.</summary>
    /// <param name="documentId">The ID of the document to delete.</param>
    /// <param name="request">The soft delete request details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SoftDeleteDocumentAsync(string documentId, SoftDeleteDocumentRequest request, CancellationToken cancellationToken);

    /// <summary>Lists documents attached to an entity.</summary>
    /// <param name="entityType">The type of entity.</param>
    /// <param name="entityId">The ID of the entity.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A paginated response of entity documents.</returns>
    Task<PaginatedResponse<EntityDocumentItemResponse>> GetEntityDocumentsAsync(
        string entityType, Guid entityId, int page, int pageSize, CancellationToken cancellationToken);

    /// <summary>Gets the absolute URL the client should redirect to for downloading.</summary>
    /// <remarks>Implementation should call the API and surface the 302 location header without following.</remarks>
    /// <param name="documentId">The ID of the document to download.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The URI to redirect to for downloading the document.</returns>
    Task<Uri> GetDownloadUrlAsync(string documentId, CancellationToken cancellationToken);
}
