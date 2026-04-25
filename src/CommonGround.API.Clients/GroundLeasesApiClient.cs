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
        int page, int pageSize, string? status, string? landParcelId, string? buildingId, string? search, CancellationToken cancellationToken)
    {
        var qs = $"?page.number={page}&page.size={pageSize}";
        if (!string.IsNullOrWhiteSpace(status))
        {
            qs += $"&filter.statuses={Uri.EscapeDataString(status)}";
        }

        if (!string.IsNullOrWhiteSpace(landParcelId))
        {
            qs += $"&filter.landParcelId={Uri.EscapeDataString(landParcelId)}";
        }

        if (!string.IsNullOrWhiteSpace(buildingId))
        {
            qs += $"&filter.buildingId={Uri.EscapeDataString(buildingId)}";
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            qs += $"&filter.search={Uri.EscapeDataString(search)}";
        }

        return await GetAsync<PaginatedResponse<GroundLeaseListItemResponse>>($"api/ground-leases{qs}", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task ChangeHomeownerAsync(string groundLeaseId, ChangeHomeownerRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);
        await PatchAsync($"api/ground-leases/{groundLeaseId}/homeowner", request, cancellationToken).ConfigureAwait(false);
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
