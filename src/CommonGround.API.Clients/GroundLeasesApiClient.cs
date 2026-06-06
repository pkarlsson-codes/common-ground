using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.Extensions.Logging;

namespace CommonGround.API.Clients;

/// <summary>HTTP client for ground lease API endpoints.</summary>
public sealed class GroundLeasesApiClient(
    HttpClient httpClient,
    ILogger<GroundLeasesApiClient> logger) : BaseApiClient(httpClient, logger), IGroundLeasesApiClient
{
    /// <inheritdoc/>
    public async Task<CreateGroundLeaseResponse> CreateGroundLeaseAsync(CreateGroundLeaseRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await PostAsync<CreateGroundLeaseRequest, CreateGroundLeaseResponse>("api/ground-leases", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<GroundLeaseResponse> GetGroundLeaseAsync(string groundLeaseId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        return await GetAsync<GroundLeaseResponse>($"api/ground-leases/{groundLeaseId}", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<PaginatedResponse<GroundLeaseListItemResponse>> GetGroundLeasesAsync(
        GetGroundLeasesQuerySpec querySpec, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(querySpec);

        return await GetAsync<PaginatedResponse<GroundLeaseListItemResponse>>($"api/ground-leases{querySpec.ToQueryString()}", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task RecordStewardshipCheckInAsync(string groundLeaseId, RecordStewardshipCheckInRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/ground-leases/{groundLeaseId}/stewardship-check-ins", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<ResaleEstimateResponse> CalculateResaleEstimateAsync(string groundLeaseId, CalculateResaleEstimateRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);
        return await PostAsync<CalculateResaleEstimateRequest, ResaleEstimateResponse>($"api/ground-leases/{groundLeaseId}/resale-estimate", request, cancellationToken).ConfigureAwait(false);
    }
}
