using CommonGround.API.Contracts.PropertyRegistry.Requests;
using CommonGround.API.Contracts.PropertyRegistry.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.Extensions.Logging;

namespace CommonGround.API.Clients;

/// <summary>
/// HTTP client for land parcel API endpoints.
/// All requests are automatically decorated with an internal JWT by <c>InternalGatewayAuthHandler</c>.
/// </summary>
public sealed class LandParcelsApiClient(
    HttpClient httpClient,
    ILogger<LandParcelsApiClient> logger) : BaseApiClient(httpClient, logger), ILandParcelsApiClient
{
    /// <inheritdoc/>
    public async Task<RegisterLandParcelResponse> RegisterLandParcelAsync(RegisterLandParcelRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await PostAsync<RegisterLandParcelRequest, RegisterLandParcelResponse>("api/land-parcels", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<LandParcelResponse> GetLandParcelAsync(string landParcelId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(landParcelId);
        return await GetAsync<LandParcelResponse>($"api/land-parcels/{landParcelId}", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<PaginatedResponse<LandParcelListItemResponse>> GetLandParcelsAsync(
        int page, int pageSize, string? status, string? search, CancellationToken cancellationToken)
    {
        var qs = $"?page.number={page}&page.size={pageSize}";
        if (!string.IsNullOrWhiteSpace(status))
        {
            qs += $"&filter.statuses={Uri.EscapeDataString(status)}";
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            qs += $"&filter.search={Uri.EscapeDataString(search)}";
        }

        return await GetAsync<PaginatedResponse<LandParcelListItemResponse>>($"api/land-parcels{qs}", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task UpdateLandParcelStatusAsync(string landParcelId, UpdateLandParcelStatusRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(landParcelId);
        ArgumentNullException.ThrowIfNull(request);
        await PatchAsync($"api/land-parcels/{landParcelId}/status", request, cancellationToken).ConfigureAwait(false);
    }
}
