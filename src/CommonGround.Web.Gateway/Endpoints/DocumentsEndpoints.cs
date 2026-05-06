using System.Text.Json;
using CommonGround.API.Clients;

using CommonGround.API.Contracts.Documents.Requests;
using CommonGround.API.Contracts.Documents.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Wolverine.Http;

#pragma warning disable CA2007
#pragma warning disable SA1600

namespace CommonGround.Web.Gateway.Endpoints;

public static class DocumentsEndpoints
{
    [WolverinePost("/api/v1/documents")]
    public static async Task<IResult> UploadDocument(
        HttpContext httpContext,
        IDocumentsApiClient client,
        CancellationToken cancellationToken)
    {
        if (!httpContext.Request.HasFormContentType)
        {
            return Results.BadRequest("multipart/form-data required");
        }

        var form = await httpContext.Request.ReadFormAsync(cancellationToken).ConfigureAwait(false);
        var metaJson = form["metadata"].ToString();
        var meta = JsonSerializer.Deserialize<UploadDocumentMetadata>(metaJson)
            ?? throw new InvalidOperationException("metadata part missing");
        var file = form.Files["file"]
            ?? throw new InvalidOperationException("file part missing");

        await using var stream = file.OpenReadStream();
        var result = await client.UploadDocumentAsync(meta, stream, file.FileName, file.ContentType, cancellationToken).ConfigureAwait(false);
        return Results.Created($"/api/v1/documents/{result.DocumentId}", result);
    }

    [WolverineGet("/api/v1/documents/{documentId}")]
    public static async Task<IResult> GetDocument(
        string documentId,
        IDocumentsApiClient client,
        CancellationToken cancellationToken)
        => Results.Ok(await client.GetDocumentAsync(documentId, cancellationToken).ConfigureAwait(false));

    [WolverinePost("/api/v1/documents/{documentId}/attachments")]
    public static async Task<IResult> AttachDocument(
        string documentId,
        AttachDocumentRequest request,
        IDocumentsApiClient client,
        CancellationToken cancellationToken)
    {
        await client.AttachDocumentAsync(documentId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverineDelete("/api/v1/documents/{documentId}")]
    public static async Task<IResult> SoftDeleteDocument(
        string documentId,
        HttpContext httpContext,
        IDocumentsApiClient client,
        CancellationToken cancellationToken)
    {
        var request = await JsonSerializer.DeserializeAsync<SoftDeleteDocumentRequest>(
            httpContext.Request.Body,
            cancellationToken: cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Request body required");
        await client.SoftDeleteDocumentAsync(documentId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverineGet("/api/v1/documents/{documentId}/download")]
    public static async Task<IResult> DownloadDocument(
        string documentId,
        IDocumentsApiClient client,
        CancellationToken cancellationToken)
    {
        var url = await client.GetDownloadUrlAsync(documentId, cancellationToken).ConfigureAwait(false);
        return Results.Redirect(url.ToString());
    }

    [WolverinePost("/api/v1/documents/{documentId}/versions")]
    public static async Task<IResult> AddDocumentVersion(
        string documentId,
        HttpContext httpContext,
        IDocumentsApiClient client,
        CancellationToken cancellationToken)
    {
        if (!httpContext.Request.HasFormContentType)
        {
            return Results.BadRequest("multipart/form-data required");
        }

        var form = await httpContext.Request.ReadFormAsync(cancellationToken).ConfigureAwait(false);
        var metaJson = form["metadata"].ToString();
        var meta = string.IsNullOrWhiteSpace(metaJson)
            ? new AddDocumentVersionMetadata()
            : JsonSerializer.Deserialize<AddDocumentVersionMetadata>(metaJson) ?? new AddDocumentVersionMetadata();
        var file = form.Files["file"]
            ?? throw new InvalidOperationException("file part missing");

        await using var stream = file.OpenReadStream();
        var result = await client.AddDocumentVersionAsync(documentId, meta, stream, file.FileName, file.ContentType, cancellationToken).ConfigureAwait(false);
        return Results.Created($"/api/v1/documents/{documentId}", result);
    }

    [WolverinePatch("/api/v1/documents/{documentId}")]
    public static async Task<IResult> UpdateDocumentMetadata(
        string documentId,
        UpdateDocumentMetadataRequest request,
        IDocumentsApiClient client,
        CancellationToken cancellationToken)
    {
        await client.UpdateDocumentMetadataAsync(documentId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/documents/{documentId}/tags")]
    public static async Task<IResult> TagDocument(
        string documentId,
        TagDocumentRequest request,
        IDocumentsApiClient client,
        CancellationToken cancellationToken)
    {
        await client.TagDocumentAsync(documentId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverineDelete("/api/v1/documents/{documentId}/tags/{tag}")]
    public static async Task<IResult> UntagDocument(
        string documentId,
        string tag,
        IDocumentsApiClient client,
        CancellationToken cancellationToken)
    {
        await client.UntagDocumentAsync(documentId, tag, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverineGet("/api/v1/documents")]
    public static async Task<IResult> ListDocuments(
        int pageNumber,
        int pageSize,
        bool includeDeleted,
        string? contentType,
        string? tag,
        DateTimeOffset? expiringBefore,
        IDocumentsApiClient client,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber <= 0)
        {
            pageNumber = 1;
        }

        if (pageSize <= 0)
        {
            pageSize = 20;
        }

        var result = await client.ListDocumentsAsync(pageNumber, pageSize, includeDeleted, contentType, tag, expiringBefore, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverineGet("/api/v1/entities/{entityType}/{entityId}/documents")]
    public static async Task<IResult> GetEntityDocuments(
        string entityType,
        Guid entityId,
        int pageNumber,
        int pageSize,
        IDocumentsApiClient client,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber <= 0)
        {
            pageNumber = 1;
        }

        if (pageSize <= 0)
        {
            pageSize = 20;
        }

        var result = await client.GetEntityDocumentsAsync(entityType, entityId, pageNumber, pageSize, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }
}
