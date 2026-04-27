using CommonGround.API.Clients;
using CommonGround.API.Contracts.Documents.Requests;
using CommonGround.API.Contracts.Documents.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.AspNetCore.Mvc;

namespace CommonGround.Web.Gateway.Controllers;

/// <summary>Gateway proxy for document endpoints.</summary>
/// <param name="client">Typed Documents API client.</param>
[ApiController]
[Route("api/v1")]
public sealed class DocumentsController(IDocumentsApiClient client) : ControllerBase
{
    /// <summary>Uploads a new document. Forwards multipart body to the API without buffering.</summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>201 Created with the uploaded document response, or 400 if multipart/form-data is missing.</returns>
    [HttpPost("documents")]
    [DisableRequestSizeLimit]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(UploadDocumentResponse), 201)]
    [EndpointName("uploadDocument")]
    public async Task<IActionResult> UploadDocument(CancellationToken cancellationToken)
    {
        if (!Request.HasFormContentType)
        {
            return BadRequest("multipart/form-data required");
        }

        var form = await Request.ReadFormAsync(cancellationToken).ConfigureAwait(false);
        var metaJson = form["metadata"].ToString();
        var meta = System.Text.Json.JsonSerializer.Deserialize<UploadDocumentMetadata>(metaJson)
            ?? throw new InvalidOperationException("metadata part missing");
        var file = form.Files["file"]
            ?? throw new InvalidOperationException("file part missing");

        await using var stream = file.OpenReadStream();
        var result = await client.UploadDocumentAsync(meta, stream, file.FileName, file.ContentType, cancellationToken)
            .ConfigureAwait(false);
        return CreatedAtAction(nameof(GetDocument), new { documentId = result.DocumentId }, result);
    }

    /// <summary>Gets a document by ID.</summary>
    /// <param name="documentId">The ID of the document to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>200 with the document response.</returns>
    [HttpGet("documents/{documentId}")]
    [ProducesResponseType(typeof(DocumentResponse), 200)]
    [EndpointName("getDocument")]
    public async Task<IActionResult> GetDocument(string documentId, CancellationToken cancellationToken)
        => Ok(await client.GetDocumentAsync(documentId, cancellationToken).ConfigureAwait(false));

    /// <summary>Attaches a document to an entity.</summary>
    /// <param name="documentId">The ID of the document to attach.</param>
    /// <param name="request">The attachment request details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content.</returns>
    [HttpPost("documents/{documentId}/attachments")]
    [ProducesResponseType(204)]
    [EndpointName("attachDocument")]
    public async Task<IActionResult> AttachDocument(
        string documentId, [FromBody] AttachDocumentRequest request, CancellationToken cancellationToken)
    {
        await client.AttachDocumentAsync(documentId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Soft-deletes a document.</summary>
    /// <param name="documentId">The ID of the document to delete.</param>
    /// <param name="request">The soft delete request details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content.</returns>
    [HttpDelete("documents/{documentId}")]
    [ProducesResponseType(204)]
    [EndpointName("softDeleteDocument")]
    public async Task<IActionResult> SoftDeleteDocument(
        string documentId, [FromBody] SoftDeleteDocumentRequest request, CancellationToken cancellationToken)
    {
        await client.SoftDeleteDocumentAsync(documentId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Redirects to the download URL for a document.</summary>
    /// <param name="documentId">The ID of the document to download.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>302 redirect to the download URL.</returns>
    [HttpGet("documents/{documentId}/download")]
    [ProducesResponseType(302)]
    [EndpointName("downloadDocument")]
    public async Task<IActionResult> Download(string documentId, CancellationToken cancellationToken)
    {
        var url = await client.GetDownloadUrlAsync(documentId, cancellationToken).ConfigureAwait(false);
        return Redirect(url.ToString());
    }

    /// <summary>Adds a new version to an existing document. Multipart/form-data identical to upload.</summary>
    /// <param name="documentId">The document ID.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>201 Created with the new version response.</returns>
    [HttpPost("documents/{documentId}/versions")]
    [DisableRequestSizeLimit]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(AddDocumentVersionResponse), 201)]
    [EndpointName("addDocumentVersion")]
    public async Task<IActionResult> AddDocumentVersion(string documentId, CancellationToken cancellationToken)
    {
        if (!Request.HasFormContentType)
        {
            return BadRequest("multipart/form-data required");
        }

        var form = await Request.ReadFormAsync(cancellationToken).ConfigureAwait(false);
        var metaJson = form["metadata"].ToString();
        var meta = string.IsNullOrWhiteSpace(metaJson)
            ? new AddDocumentVersionMetadata()
            : System.Text.Json.JsonSerializer.Deserialize<AddDocumentVersionMetadata>(metaJson) ?? new AddDocumentVersionMetadata();
        var file = form.Files["file"]
            ?? throw new InvalidOperationException("file part missing");

        await using var stream = file.OpenReadStream();
        var result = await client.AddDocumentVersionAsync(
            documentId, meta, stream, file.FileName, file.ContentType, cancellationToken).ConfigureAwait(false);
        return CreatedAtAction(nameof(GetDocument), new { documentId }, result);
    }

    /// <summary>Updates editable metadata fields on a document.</summary>
    /// <param name="documentId">The document ID.</param>
    /// <param name="request">The metadata update request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content.</returns>
    [HttpPatch("documents/{documentId}")]
    [ProducesResponseType(204)]
    [EndpointName("updateDocumentMetadata")]
    public async Task<IActionResult> UpdateDocumentMetadata(
        string documentId, [FromBody] UpdateDocumentMetadataRequest request, CancellationToken cancellationToken)
    {
        await client.UpdateDocumentMetadataAsync(documentId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Adds a tag to a document.</summary>
    /// <param name="documentId">The document ID.</param>
    /// <param name="request">The tag request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content.</returns>
    [HttpPost("documents/{documentId}/tags")]
    [ProducesResponseType(204)]
    [EndpointName("tagDocument")]
    public async Task<IActionResult> TagDocument(
        string documentId, [FromBody] TagDocumentRequest request, CancellationToken cancellationToken)
    {
        await client.TagDocumentAsync(documentId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Removes a tag from a document.</summary>
    /// <param name="documentId">The document ID.</param>
    /// <param name="tag">The tag to remove.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content.</returns>
    [HttpDelete("documents/{documentId}/tags/{tag}")]
    [ProducesResponseType(204)]
    [EndpointName("untagDocument")]
    public async Task<IActionResult> UntagDocument(
        string documentId, string tag, CancellationToken cancellationToken)
    {
        await client.UntagDocumentAsync(documentId, tag, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Lists documents in the library, paginated with optional filters.</summary>
    /// <param name="page">The 1-based page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="includeDeleted">When true, includes soft-deleted documents.</param>
    /// <param name="contentType">Optional MIME content-type filter.</param>
    /// <param name="tag">Optional tag filter.</param>
    /// <param name="expiringBefore">When provided, restricts results to documents expiring before this date.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>200 with the paginated library list.</returns>
    [HttpGet("documents")]
    [ProducesResponseType(typeof(PaginatedResponse<DocumentLibraryItemResponse>), 200)]
    [EndpointName("listDocuments")]
    public async Task<IActionResult> ListDocuments(
        [FromQuery(Name = "page.number")] int page = 1,
        [FromQuery(Name = "page.size")] int pageSize = 20,
        [FromQuery(Name = "includeDeleted")] bool includeDeleted = false,
        [FromQuery(Name = "contentType")] string? contentType = null,
        [FromQuery(Name = "tag")] string? tag = null,
        [FromQuery(Name = "expiringBefore")] DateTimeOffset? expiringBefore = null,
        CancellationToken cancellationToken = default)
        => Ok(await client.ListDocumentsAsync(page, pageSize, includeDeleted, contentType, tag, expiringBefore, cancellationToken).ConfigureAwait(false));

    /// <summary>Lists documents attached to an entity.</summary>
    /// <param name="entityType">The type of entity.</param>
    /// <param name="entityId">The ID of the entity.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>200 with a paginated response of entity documents.</returns>
    [HttpGet("entities/{entityType}/{entityId}/documents")]
    [ProducesResponseType(typeof(PaginatedResponse<EntityDocumentItemResponse>), 200)]
    [EndpointName("getEntityDocuments")]
    public async Task<IActionResult> GetEntityDocuments(
        string entityType, Guid entityId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
        => Ok(await client.GetEntityDocumentsAsync(entityType, entityId, page, pageSize, cancellationToken).ConfigureAwait(false));
}
