using System.Text.Json;
using CommonGround.API.Clients;
using CommonGround.API.Contracts.Documents.Requests;
using CommonGround.API.Contracts.Documents.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace CommonGround.Management.Gateway.Endpoints;

public static class DocumentsEndpoints
{
    private static readonly string[] DownloadHeaders =
        ["Content-Disposition", "Content-Length", "X-Content-Type-Options", "Content-Security-Policy"];

    private static readonly JsonSerializerOptions _webJson = new(JsonSerializerDefaults.Web);

    [WolverinePost("/api/v1/documents")]
    [ProducesResponseType<UploadDocumentResponse>(StatusCodes.Status201Created)]
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
        var meta = JsonSerializer.Deserialize<UploadDocumentMetadata>(metaJson, _webJson)
            ?? throw new InvalidOperationException("metadata part missing");
        var file = form.Files["file"]
            ?? throw new InvalidOperationException("file part missing");

        await using var stream = file.OpenReadStream();
        var result = await client.UploadDocumentAsync(meta, stream, file.FileName, file.ContentType, cancellationToken).ConfigureAwait(false);
        return Results.Created($"/api/v1/documents/{result.DocumentId}", result);
    }

    [WolverineGet("/api/v1/documents/{documentId}")]
    [ProducesResponseType<DocumentResponse>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetDocument(
        string documentId,
        IDocumentsApiClient client,
        CancellationToken cancellationToken)
        => Results.Ok(await client.GetDocumentAsync(documentId, cancellationToken).ConfigureAwait(false));

    [WolverinePost("/api/v1/documents/{documentId}/attachments")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> SoftDeleteDocument(
        string documentId,
        SoftDeleteDocumentRequest request,
        IDocumentsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await client.SoftDeleteDocumentAsync(documentId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverineGet("/api/v1/documents/{documentId}/download")]
    public static async Task<IResult> DownloadDocument(
        string documentId,
        string? versionId,
        HttpContext httpContext,
        IDocumentsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var download = await client.GetDownloadAsync(documentId, versionId, cancellationToken).ConfigureAwait(false);
        httpContext.Response.RegisterForDispose(download);
        if (download.RedirectUrl is { } url)
        {
            return Results.Redirect(url.ToString());
        }

        // The file system store: pass the API's file through with the headers it set (disposition, nosniff, CSP sandbox).
        var file = download.FileResponse!;
        var headers = httpContext.Response.Headers;
        foreach (var name in DownloadHeaders)
        {
            if (file.Headers.TryGetValues(name, out var values) || file.Content.Headers.TryGetValues(name, out values))
            {
                headers[name] = values.ToArray();
            }
        }

        var body = await file.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return Results.Stream(body, file.Content.Headers.ContentType?.ToString());
    }

    [WolverinePost("/api/v1/documents/{documentId}/versions")]
    [ProducesResponseType<AddDocumentVersionResponse>(StatusCodes.Status201Created)]
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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
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
    [ProducesResponseType<PaginatedResponse<DocumentLibraryItemResponse>>(StatusCodes.Status200OK)]
    public static async Task<IResult> ListDocuments(
        [FromQuery] GetDocumentsQuerySpec querySpec,
        IDocumentsApiClient client,
        CancellationToken cancellationToken = default)
    {
        var result = await client.ListDocumentsAsync(querySpec, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverineGet("/api/v1/entities/{entityType}/{entityId}/documents")]
    [ProducesResponseType<PaginatedResponse<EntityDocumentItemResponse>>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetEntityDocuments(
        string entityType,
        string entityId,
        [FromQuery] GetEntityDocumentsQuerySpec querySpec,
        IDocumentsApiClient client,
        CancellationToken cancellationToken = default)
    {
        var result = await client.GetEntityDocumentsAsync(entityType, entityId, querySpec, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }
}
