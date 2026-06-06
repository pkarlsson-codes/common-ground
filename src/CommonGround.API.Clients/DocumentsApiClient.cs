using System.Net.Http.Headers;
using System.Net.Http.Json;
using CommonGround.API.Contracts.Documents.Requests;
using CommonGround.API.Contracts.Documents.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.Extensions.Logging;

namespace CommonGround.API.Clients;

/// <summary>HTTP client for the Documents API.</summary>
public sealed class DocumentsApiClient(
    HttpClient httpClient,
    ILogger<DocumentsApiClient> logger)
    : BaseApiClient(httpClient, logger), IDocumentsApiClient
{
    /// <inheritdoc/>
    public async Task<UploadDocumentResponse> UploadDocumentAsync(
        UploadDocumentMetadata metadata,
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(content);

        using var form = new MultipartFormDataContent();
        form.Add(JsonContent.Create(metadata), "metadata");

        var fileContent = new StreamContent(content);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
        form.Add(fileContent, "file", fileName);

        using var response = await HttpClient.PostAsync("api/documents", form, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<UploadDocumentResponse>(cancellationToken).ConfigureAwait(false))!;
    }

    /// <inheritdoc/>
    public Task<DocumentResponse> GetDocumentAsync(string documentId, CancellationToken cancellationToken)
        => GetAsync<DocumentResponse>($"api/documents/{documentId}", cancellationToken);

    /// <inheritdoc/>
    public Task AttachDocumentAsync(string documentId, AttachDocumentRequest request, CancellationToken cancellationToken)
        => PostAsync($"api/documents/{documentId}/attachments", request, cancellationToken);

    /// <inheritdoc/>
    public async Task SoftDeleteDocumentAsync(string documentId, SoftDeleteDocumentRequest request, CancellationToken cancellationToken)
    {
        using var msg = new HttpRequestMessage(HttpMethod.Delete, $"api/documents/{documentId}")
        {
            Content = JsonContent.Create(request),
        };
        using var response = await HttpClient.SendAsync(msg, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    /// <inheritdoc/>
    public async Task<AddDocumentVersionResponse> AddDocumentVersionAsync(
        string documentId,
        AddDocumentVersionMetadata metadata,
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentId);
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(content);

        using var form = new MultipartFormDataContent();
        form.Add(JsonContent.Create(metadata), "metadata");
        var fileContent = new StreamContent(content);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
        form.Add(fileContent, "file", fileName);

        using var response = await HttpClient.PostAsync(
            $"api/documents/{documentId}/versions", form, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AddDocumentVersionResponse>(cancellationToken).ConfigureAwait(false))!;
    }

    /// <inheritdoc/>
    public Task UpdateDocumentMetadataAsync(string documentId, UpdateDocumentMetadataRequest request, CancellationToken cancellationToken)
        => PatchAsync($"api/documents/{documentId}", request, cancellationToken);

    /// <inheritdoc/>
    public Task TagDocumentAsync(string documentId, TagDocumentRequest request, CancellationToken cancellationToken)
        => PostAsync($"api/documents/{documentId}/tags", request, cancellationToken);

    /// <inheritdoc/>
    public Task UntagDocumentAsync(string documentId, string tag, CancellationToken cancellationToken)
        => DeleteAsync($"api/documents/{documentId}/tags/{Uri.EscapeDataString(tag)}", cancellationToken);

    /// <inheritdoc/>
    public Task<PaginatedResponse<DocumentLibraryItemResponse>> ListDocumentsAsync(
        int pageNumber, int pageSize, bool includeDeleted, string? contentType, string? tag,
        DateTimeOffset? expiringBefore, CancellationToken cancellationToken)
    {
        var query = new List<string>(capacity: 6)
        {
            $"pageNumber={pageNumber}",
            $"pageSize={pageSize}",
        };
        if (includeDeleted)
        {
            query.Add("includeDeleted=true");
        }

        if (!string.IsNullOrWhiteSpace(contentType))
        {
            query.Add($"contentType={Uri.EscapeDataString(contentType)}");
        }

        if (!string.IsNullOrWhiteSpace(tag))
        {
            query.Add($"tag={Uri.EscapeDataString(tag)}");
        }

        if (expiringBefore.HasValue)
        {
            query.Add($"expiringBefore={Uri.EscapeDataString(expiringBefore.Value.ToString("O"))}");
        }

        return GetAsync<PaginatedResponse<DocumentLibraryItemResponse>>(
            $"api/documents?{string.Join('&', query)}", cancellationToken);
    }

    /// <inheritdoc/>
    public Task<PaginatedResponse<EntityDocumentItemResponse>> GetEntityDocumentsAsync(
        string entityType, string entityId, int pageNumber, int pageSize, CancellationToken cancellationToken)
        => GetAsync<PaginatedResponse<EntityDocumentItemResponse>>(
            $"api/entities/{entityType}/{Uri.EscapeDataString(entityId)}/documents?pageNumber={pageNumber}&pageSize={pageSize}",
            cancellationToken);

    /// <inheritdoc/>
    public async Task<Uri> GetDownloadUrlAsync(string documentId, CancellationToken cancellationToken)
    {
        using var clientNoRedirect = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false })
        {
            BaseAddress = HttpClient.BaseAddress,
        };
        if (HttpClient.DefaultRequestHeaders.Authorization is { } auth)
        {
            clientNoRedirect.DefaultRequestHeaders.Authorization = auth;
        }

        using var resp = await clientNoRedirect.GetAsync(
            $"api/documents/{documentId}/download", cancellationToken).ConfigureAwait(false);

        if ((int)resp.StatusCode is not(>= 300 and < 400) || resp.Headers.Location is null)
        {
            resp.EnsureSuccessStatusCode();
        }

        return resp.Headers.Location!;
    }
}
