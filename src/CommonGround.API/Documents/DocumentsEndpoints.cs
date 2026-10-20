using System.Text.Json;
using CommonGround.API.Contracts.Documents.Requests;
using CommonGround.API.Contracts.Documents.Responses;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.Documents.Application.Commands;
using CommonGround.Documents.Application.Queries;
using CommonGround.Documents.Application.ReadModels;
using CommonGround.Documents.Domain.Documents;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace CommonGround.API.Documents;
public static class DocumentsEndpoints
{
    private static readonly JsonSerializerOptions MetadataJson = new(JsonSerializerDefaults.Web);

    [WolverinePost("/api/documents")]
    public static async Task<IResult> UploadDocument(HttpContext httpContext, IMessageBus bus, IDocumentBlobStore blobs, CancellationToken cancellationToken)
    {
        if (!httpContext.Request.HasFormContentType)
        {
            return Results.BadRequest("multipart/form-data required");
        }

        var form = await httpContext.Request.ReadFormAsync(cancellationToken).ConfigureAwait(false);

        var metadataPart = form["metadata"].ToString();
        if (string.IsNullOrWhiteSpace(metadataPart))
        {
            return Results.BadRequest("'metadata' part missing");
        }

        var metadata = JsonSerializer.Deserialize<UploadDocumentMetadata>(metadataPart, MetadataJson)
            ?? throw new InvalidOperationException("metadata part could not be deserialised");

        var file = form.Files["file"]
            ?? throw new InvalidOperationException("'file' part missing");

        var userId = httpContext.GetUserId();

        var documentId = DocumentId.Generate();
        var versionId = DocumentVersionId.Generate();
        var ext = Path.GetExtension(file.FileName);
        var blobKey = $"documents/{documentId.Value}/{versionId.Value}{ext}";

        await using (var stream = file.OpenReadStream())
        {
            var uploadResult =
                await blobs.UploadAsync(stream, file.ContentType, blobKey, cancellationToken).ConfigureAwait(false);

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

            var assignedId = await bus.InvokeForTenantAsync<DocumentId>(bus.TenantId!, command, cancellationToken).ConfigureAwait(false);

            return Results.Created($"/api/documents/{assignedId.Value}", new UploadDocumentResponse(assignedId.Value));
        }
    }

    [WolverineGet("/api/documents/{documentId}")]
    public static async Task<IResult> GetDocument(string documentId, HttpContext httpContext, IMessageBus bus, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentId);

        var view = await bus.InvokeForTenantAsync<DocumentView>(bus.TenantId!,
            new GetDocument(DocumentId.Create(documentId)), cancellationToken).ConfigureAwait(false);

        DocumentVersionResponse[] versions = [.. view.Versions.Select(v => new DocumentVersionResponse(
            v.VersionId, v.VersionNumber, v.FileName, v.ContentType, v.SizeBytes, v.Sha256, v.UploadedAt))];
        DocumentAttachmentResponse[] attachments = [.. view.Attachments.Select(a => new DocumentAttachmentResponse(
            a.EntityType, a.EntityId, a.Role, a.AttachedAt))];

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
            view.ExpiresAt,
            [.. view.Tags],
            versions,
            attachments);

        return Results.Ok(response);
    }

    [WolverinePost("/api/documents/{documentId}/attachments")]
    public static async Task<IResult> AttachDocument(string documentId, AttachDocumentRequest request, HttpContext httpContext, IMessageBus bus, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new AttachDocument(
                DocumentId.Create(documentId),
                request.EntityType,
                request.EntityId,
                request.Role,
                userId),
            cancellationToken).ConfigureAwait(false);

        return Results.NoContent();
    }

    [WolverineDelete("/api/documents/{documentId}")]
    public static async Task<IResult> SoftDeleteDocument(string documentId, SoftDeleteDocumentRequest request, HttpContext httpContext, IMessageBus bus, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();
        await bus.InvokeForTenantAsync(bus.TenantId!,
            new SoftDeleteDocument(DocumentId.Create(documentId), request.Reason, userId),
            cancellationToken).ConfigureAwait(false);

        return Results.NoContent();
    }

    [WolverineGet("/api/documents/{documentId}/download")]
    public static async Task<IResult> Download(string documentId, string? versionId, HttpContext httpContext, IMessageBus bus, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentId);

        var url = await bus.InvokeForTenantAsync<Uri>(bus.TenantId!,
            new GetDocumentDownloadUrl(DocumentId.Create(documentId), versionId), cancellationToken).ConfigureAwait(false);

        return Results.Redirect(url.ToString());
    }

    [WolverinePost("/api/documents/{documentId}/versions")]
    public static async Task<IResult> AddDocumentVersion(string documentId, HttpContext httpContext, IMessageBus bus, IDocumentBlobStore blobs, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentId);

        if (!httpContext.Request.HasFormContentType)
        {
            return Results.BadRequest("multipart/form-data required");
        }

        var form = await httpContext.Request.ReadFormAsync(cancellationToken).ConfigureAwait(false);
        var metadataPart = form["metadata"].ToString();
        var metadata = string.IsNullOrWhiteSpace(metadataPart)
            ? new AddDocumentVersionMetadata()
            : JsonSerializer.Deserialize<AddDocumentVersionMetadata>(metadataPart, MetadataJson)
                ?? new AddDocumentVersionMetadata();

        var file = form.Files["file"]
            ?? throw new InvalidOperationException("'file' part missing");

        var userId = httpContext.GetUserId();
        var docId = DocumentId.Create(documentId);
        var versionId = DocumentVersionId.Generate();
        var ext = Path.GetExtension(file.FileName);
        var blobKey = $"documents/{docId.Value}/{versionId.Value}{ext}";

        await using var stream = file.OpenReadStream();
        var uploadResult = await blobs.UploadAsync(stream, file.ContentType, blobKey, cancellationToken).ConfigureAwait(false);

        var command = new AddDocumentVersion(
            docId,
            uploadResult.BlobKey,
            file.FileName,
            file.ContentType,
            uploadResult.SizeBytes,
            uploadResult.Sha256,
            metadata.Note,
            userId,
            versionId);

        var assignedVersionId = await bus.InvokeForTenantAsync<DocumentVersionId>(bus.TenantId!, command, cancellationToken).ConfigureAwait(false);

        return Results.Created($"/api/documents/{docId.Value}", new AddDocumentVersionResponse(docId.Value, assignedVersionId.Value));
    }

    [WolverinePatch("/api/documents/{documentId}")]
    public static async Task<IResult> UpdateDocumentMetadata(string documentId, UpdateDocumentMetadataRequest request, HttpContext httpContext, IMessageBus bus, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();
        var sensitivity = string.IsNullOrWhiteSpace(request.Sensitivity)
            ? null
            : SensitivityLevel.FromDisplayName(request.Sensitivity);

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new UpdateDocumentMetadata(
                DocumentId.Create(documentId),
                request.Title,
                sensitivity,
                request.ExpiresAt,
                request.ClearExpiresAt,
                userId),
            cancellationToken).ConfigureAwait(false);

        return Results.NoContent();
    }

    [WolverinePost("/api/documents/{documentId}/tags")]
    public static async Task<IResult> TagDocument(string documentId, TagDocumentRequest request, HttpContext httpContext, IMessageBus bus, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();
        await bus.InvokeForTenantAsync(bus.TenantId!,
            new TagDocument(DocumentId.Create(documentId), request.Tag, userId),
            cancellationToken).ConfigureAwait(false);

        return Results.NoContent();
    }

    [WolverineDelete("/api/documents/{documentId}/tags/{tag}")]
    public static async Task<IResult> UntagDocument(string documentId, string tag, HttpContext httpContext, IMessageBus bus, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentId);
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);

        var userId = httpContext.GetUserId();
        await bus.InvokeForTenantAsync(bus.TenantId!,
            new UntagDocument(DocumentId.Create(documentId), tag, userId),
            cancellationToken).ConfigureAwait(false);

        return Results.NoContent();
    }

    [WolverineGet("/api/documents")]
    public static async Task<IResult> ListDocuments(
        [AsParameters] GetDocumentsQuerySpec spec,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken = default)
    {
        var result = await bus.InvokeForTenantAsync<PaginatedResponse<DocumentLibraryItemView>>(
            bus.TenantId!,
            new ListDocuments(spec.Page, spec.IncludeDeleted, spec.ContentType, spec.Tag, spec.ExpiringBefore),
            cancellationToken).ConfigureAwait(false);

        return Results.Ok(result.MapItems(d => new DocumentLibraryItemResponse(
            d.Id, d.Title, d.Sensitivity, d.Retention,
            d.CurrentFileName, d.CurrentContentType, d.CurrentSizeBytes, d.CurrentVersionNumber,
            d.UploadedBy, d.UploadedAt, d.LastModifiedAt, d.IsDeleted, d.ExpiresAt,
            [.. d.Tags])));
    }

    [WolverineGet("/api/entities/{entityType}/{entityId}/documents")]
    public static async Task<IResult> GetEntityDocuments(
        string entityType,
        string entityId,
        [FromQuery] GetEntityDocumentsQuerySpec request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);

        var result = await bus.InvokeForTenantAsync<PaginatedResponse<EntityDocumentAttachmentItem>>(
            bus.TenantId!,
            new GetEntityDocuments(entityType, entityId, request.Page, request.IncludeDeleted),
            cancellationToken).ConfigureAwait(false);

        return Results.Ok(result.MapItems(i => new EntityDocumentItemResponse(
            i.DocumentId,
            i.Title,
            i.CurrentFileName,
            i.CurrentContentType,
            i.CurrentSizeBytes,
            i.Role,
            i.AttachedAt)));
    }
}
