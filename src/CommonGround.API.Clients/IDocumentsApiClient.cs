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

    /// <summary>Adds a new version to a document via multipart upload.</summary>
    /// <param name="documentId">The document ID.</param>
    /// <param name="metadata">Optional version metadata (note).</param>
    /// <param name="content">The file stream content.</param>
    /// <param name="fileName">The name of the file being uploaded.</param>
    /// <param name="contentType">The MIME type of the file.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The new version response.</returns>
    Task<AddDocumentVersionResponse> AddDocumentVersionAsync(
        string documentId,
        AddDocumentVersionMetadata metadata,
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken);

    /// <summary>Updates editable metadata on a document.</summary>
    /// <param name="documentId">The document ID.</param>
    /// <param name="request">The metadata update request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateDocumentMetadataAsync(string documentId, UpdateDocumentMetadataRequest request, CancellationToken cancellationToken);

    /// <summary>Adds a tag to a document.</summary>
    /// <param name="documentId">The document ID.</param>
    /// <param name="request">The tag request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task TagDocumentAsync(string documentId, TagDocumentRequest request, CancellationToken cancellationToken);

    /// <summary>Removes a tag from a document.</summary>
    /// <param name="documentId">The document ID.</param>
    /// <param name="tag">The tag to remove (normalised lowercase).</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UntagDocumentAsync(string documentId, string tag, CancellationToken cancellationToken);

    /// <summary>Lists documents in the library, paginated with optional filters.</summary>
    /// <param name="querySpec">The pagination and filter parameters.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A paginated response of library items.</returns>
    Task<PaginatedResponse<DocumentLibraryItemResponse>> ListDocumentsAsync(
        GetDocumentsQuerySpec querySpec,
        CancellationToken cancellationToken);

    /// <summary>Lists documents attached to an entity.</summary>
    /// <param name="entityType">The type of entity.</param>
    /// <param name="entityId">The ID of the entity.</param>
    /// <param name="querySpec">The pagination and filter parameters.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A paginated response of entity documents.</returns>
    Task<PaginatedResponse<EntityDocumentItemResponse>> GetEntityDocumentsAsync(
        string entityType, string entityId, GetEntityDocumentsQuerySpec querySpec, CancellationToken cancellationToken);

    /// <summary>Gets the absolute URL the client should redirect to for downloading.</summary>
    /// <remarks>Surfaces the API's 302 location header, so the HttpClient must be configured not to follow redirects.</remarks>
    /// <param name="documentId">The ID of the document to download.</param>
    /// <param name="versionId">The version to download, or null for the current version.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The URI to redirect to for downloading the document.</returns>
    Task<Uri> GetDownloadUrlAsync(string documentId, string? versionId, CancellationToken cancellationToken);
}
