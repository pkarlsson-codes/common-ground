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
    public Task<PaginatedResponse<EntityDocumentItemResponse>> GetEntityDocumentsAsync(
        string entityType, Guid entityId, int page, int pageSize, CancellationToken cancellationToken)
        => GetAsync<PaginatedResponse<EntityDocumentItemResponse>>(
            $"api/entities/{entityType}/{entityId:N}/documents?page.number={page}&page.size={pageSize}",
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
