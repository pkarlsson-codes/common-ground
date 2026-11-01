using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.Extensions.Logging;

namespace CommonGround.API.Clients;

/// <summary>
/// HTTP client for ground lease template endpoints.
/// All requests are automatically decorated with an internal JWT by <c>InternalGatewayAuthHandler</c>.
/// </summary>
public sealed class GroundLeaseTemplatesApiClient(
    IHttpClientFactory httpClientFactory,
    ILogger<GroundLeaseTemplatesApiClient> logger) : BaseApiClient(httpClientFactory.CreateClient(nameof(GroundLeaseTemplatesApiClient)), logger), IGroundLeaseTemplatesApiClient
{
    /// <inheritdoc/>
    public async Task<PaginatedResponse<GroundLeaseTemplateListItemResponse>> GetGroundLeaseTemplatesAsync(
        GetGroundLeaseTemplatesQuerySpec querySpec, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(querySpec);

        return await GetAsync<PaginatedResponse<GroundLeaseTemplateListItemResponse>>(
            $"api/ground-lease-templates{querySpec.ToQueryString()}", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<CreateGroundLeaseTemplateResponse> CreateGroundLeaseTemplateAsync(
        CreateGroundLeaseTemplateRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await PostAsync<CreateGroundLeaseTemplateRequest, CreateGroundLeaseTemplateResponse>(
            "api/ground-lease-templates", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<GroundLeaseTemplateResponse> GetGroundLeaseTemplateAsync(string id, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return await GetAsync<GroundLeaseTemplateResponse>($"api/ground-lease-templates/{id}", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task ArchiveGroundLeaseTemplateAsync(string id, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        await PostAsync($"api/ground-lease-templates/{id}/archive", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task RestoreGroundLeaseTemplateAsync(string id, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        await PostAsync($"api/ground-lease-templates/{id}/restore", cancellationToken).ConfigureAwait(false);
    }
}
