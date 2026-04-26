using System.Text.Json;
using CommonGround.API.Contracts.Documents.Requests;
using CommonGround.API.Contracts.Documents.Responses;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.Documents.Application.Commands;
using CommonGround.Documents.Application.Queries;
using CommonGround.Documents.Application.ReadModels;
using CommonGround.Documents.Domain.Documents;
using CommonGround.SharedKernel.Infrastructure.API;
using CommonGround.SharedKernel.Infrastructure.Querying;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CommonGround.API.Documents;

/// <summary>API controller for document management.</summary>
/// <param name="bus">The Wolverine message bus.</param>
/// <param name="blobs">The document blob store (for direct streaming during multipart upload).</param>
[ApiController]
public sealed class DocumentsController(IMessageBus bus, IDocumentBlobStore blobs) : ControllerBase
{
    private static readonly JsonSerializerOptions MetadataJson = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Uploads a new document. Request must be <c>multipart/form-data</c> with two parts:
    /// <list type="bullet">
    ///   <item><c>metadata</c> — JSON, deserialised as <see cref="UploadDocumentMetadata"/>.</item>
    ///   <item><c>file</c> — the binary content.</item>
    /// </list>
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created upload response with the document ID.</returns>
    [HttpPost("/api/documents")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(UploadDocumentResponse), 201)]
    [RequestSizeLimit(1L << 30)] // 1 GiB cap; tune in slice 2
    [RequestFormLimits(MultipartBodyLengthLimit = 1L << 30)]
    [EndpointName("uploadDocument")]
    public async Task<IActionResult> UploadDocument(CancellationToken cancellationToken)
    {
        if (!Request.HasFormContentType)
        {
            return BadRequest("multipart/form-data required");
        }

        var form = await Request.ReadFormAsync(cancellationToken).ConfigureAwait(false);

        var metadataPart = form["metadata"].ToString();
        if (string.IsNullOrWhiteSpace(metadataPart))
        {
            return BadRequest("'metadata' part missing");
        }

        var metadata = JsonSerializer.Deserialize<UploadDocumentMetadata>(metadataPart, MetadataJson)
            ?? throw new InvalidOperationException("metadata part could not be deserialised");

        var file = form.Files["file"]
            ?? throw new InvalidOperationException("'file' part missing");

        var userId = HttpContext.GetUserId();

        // Compose a deterministic key — see plan: documents/{docId}/{versionId}.{ext}.
        // We don't yet have either ID; mint placeholders here, then thread them into the command.
        // The aggregate generates its own IDs from events, so we need to stage *before* upload OR
        // accept that the aggregate's IDs differ from the keys we wrote. Cleanest: generate IDs
        // here, pre-compute the key, and pass them down.
        var documentId = DocumentId.Generate();
        var versionId = DocumentVersionId.Generate();
        var ext = Path.GetExtension(file.FileName);
        var blobKey = $"documents/{documentId.Value}/{versionId.Value}{ext}";

        await using (var stream = file.OpenReadStream())
        {
            var uploadResult =
                await blobs.UploadAsync(stream, file.ContentType, blobKey, cancellationToken).ConfigureAwait(false);

            // Pass the ALREADY-GENERATED ids and the upload metadata to the aggregate.
            var initialAttachment = metadata.InitialAttachment is { } a
                ? new UploadDocumentAttachment(a.EntityType, a.EntityId, a.Role)
                : null;

            var command = new UploadDocument(
                metadata.Title,
                SensitivityLevel.FromDisplayName(metadata.Sensitivity),
                RetentionClass.FromDisplayName(metadata.Retention),
                uploadResult.BlobKey,
                file.FileName,
                file.ContentType,
                uploadResult.SizeBytes,
                uploadResult.Sha256,
                userId,
                initialAttachment,
                documentId,
                versionId);

            var assignedId = await bus.InvokeAsync<DocumentId>(command, cancellationToken).ConfigureAwait(false);

            return CreatedAtAction(
                nameof(GetDocument),
                new { documentId = assignedId.Value },
                new UploadDocumentResponse(assignedId.Value));
        }
    }

    /// <summary>Gets a single document by ID.</summary>
    /// <param name="documentId">The document ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The document response with full details.</returns>
    [HttpGet("/api/documents/{documentId}")]
    [ProducesResponseType(typeof(DocumentResponse), 200)]
    [ProducesResponseType(404)]
    [EndpointName("getDocument")]
    public async Task<IActionResult> GetDocument(string documentId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentId);

        var view = await bus.InvokeAsync<DocumentView>(
            new GetDocument(DocumentId.Create(documentId)), cancellationToken).ConfigureAwait(false);

        var response = new DocumentResponse(
            view.Id,
            view.Title,
            view.Sensitivity,
            view.Retention,
            view.CurrentVersionId,
            view.CurrentFileName,
            view.CurrentContentType,
            view.CurrentSizeBytes,
            view.UploadedBy,
            view.UploadedAt,
            view.IsDeleted,
            [.. view.Attachments.Select(a => new DocumentAttachmentResponse(a.EntityType, a.EntityId, a.Role, a.AttachedAt))]);

        return Ok(response);
    }

    /// <summary>Attaches an existing document to an entity.</summary>
    /// <param name="documentId">The document ID.</param>
    /// <param name="request">The attachment request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content on success.</returns>
    [HttpPost("/api/documents/{documentId}/attachments")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [EndpointName("attachDocument")]
    public async Task<IActionResult> AttachDocument(
        string documentId,
        [FromBody] AttachDocumentRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = HttpContext.GetUserId();

        await bus.InvokeAsync(
            new AttachDocument(
                DocumentId.Create(documentId),
                request.EntityType,
                request.EntityId,
                request.Role,
                userId),
            cancellationToken).ConfigureAwait(false);

        return NoContent();
    }

    /// <summary>Soft-deletes a document.</summary>
    /// <param name="documentId">The document ID.</param>
    /// <param name="request">The soft delete request with reason.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("/api/documents/{documentId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [EndpointName("softDeleteDocument")]
    public async Task<IActionResult> SoftDeleteDocument(
        string documentId,
        [FromBody] SoftDeleteDocumentRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = HttpContext.GetUserId();
        await bus.InvokeAsync(
            new SoftDeleteDocument(DocumentId.Create(documentId), request.Reason, userId),
            cancellationToken).ConfigureAwait(false);

        return NoContent();
    }

    /// <summary>Returns 302 to a presigned MinIO URL for the current version's blob.</summary>
    /// <param name="documentId">The document ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A redirect to the presigned download URL.</returns>
    [HttpGet("/api/documents/{documentId}/download")]
    [ProducesResponseType(302)]
    [ProducesResponseType(404)]
    [EndpointName("downloadDocument")]
    public async Task<IActionResult> Download(string documentId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentId);

        var url = await bus.InvokeAsync<Uri>(
            new GetDocumentDownloadUrl(DocumentId.Create(documentId)), cancellationToken).ConfigureAwait(false);

        return Redirect(url.ToString());
    }

    /// <summary>Lists documents attached to a given entity.</summary>
    /// <param name="entityType">Opaque entity type (e.g. "Building"). Case-sensitive.</param>
    /// <param name="entityId">The entity's GUID identifier.</param>
    /// <param name="request">The pagination query spec.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated list of attached documents for the entity.</returns>
    [HttpGet("/api/entities/{entityType}/{entityId}/documents")]
    [ProducesResponseType(typeof(PaginatedResponse<EntityDocumentItemResponse>), 200)]
    [EndpointName("getEntityDocuments")]
    public async Task<IActionResult> GetEntityDocuments(
        string entityType,
        Guid entityId,
        [FromQuery] PaginationQuerySpec<object> request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);

        var result = await bus.InvokeAsync<PaginatedResponse<EntityDocumentAttachmentItem>>(
            new GetEntityDocuments(entityType, entityId, request.Page),
            cancellationToken).ConfigureAwait(false);

        return Ok(result.MapItems(i => new EntityDocumentItemResponse(
            i.DocumentId,
            i.Title,
            i.CurrentFileName,
            i.CurrentContentType,
            i.CurrentSizeBytes,
            i.Role,
            i.AttachedAt)));
    }
}
