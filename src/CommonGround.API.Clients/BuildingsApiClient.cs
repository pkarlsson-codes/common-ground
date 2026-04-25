using CommonGround.API.Contracts.PropertyRegistry.Requests;
using CommonGround.API.Contracts.PropertyRegistry.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.Extensions.Logging;

namespace CommonGround.API.Clients;

/// <summary>
/// HTTP client for building API endpoints.
/// All requests are automatically decorated with an internal JWT by <c>InternalGatewayAuthHandler</c>.
/// </summary>
public sealed class BuildingsApiClient(
    HttpClient httpClient,
    ILogger<BuildingsApiClient> logger) : BaseApiClient(httpClient, logger), IBuildingsApiClient
{
    /// <inheritdoc/>
    public async Task<RegisterBuildingResponse> RegisterBuildingAsync(RegisterBuildingRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await PostAsync<RegisterBuildingRequest, RegisterBuildingResponse>("api/buildings", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<BuildingResponse> GetBuildingAsync(string buildingId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(buildingId);
        return await GetAsync<BuildingResponse>($"api/buildings/{buildingId}", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<PaginatedResponse<BuildingListItemResponse>> GetBuildingsAsync(
        int page, int pageSize, string? landParcelId, string? status, CancellationToken cancellationToken)
    {
        var qs = $"?page.number={page}&page.size={pageSize}";
        if (!string.IsNullOrWhiteSpace(landParcelId))
        {
            qs += $"&filter.landParcelIds={Uri.EscapeDataString(landParcelId)}";
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            qs += $"&filter.statuses={Uri.EscapeDataString(status)}";
        }

        return await GetAsync<PaginatedResponse<BuildingListItemResponse>>($"api/buildings{qs}", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task RecordBuildingAssessmentAsync(string buildingId, RecordBuildingAssessmentRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(buildingId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/buildings/{buildingId}/assessments", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task UpdateBuildingStatusAsync(string buildingId, UpdateBuildingStatusRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(buildingId);
        ArgumentNullException.ThrowIfNull(request);
        await PatchAsync($"api/buildings/{buildingId}/status", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<PaginatedResponse<BuildingAssessmentItemResponse>> GetBuildingAssessmentsAsync(
        string buildingId, int page, int pageSize, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(buildingId);
        var qs = $"?page.number={page}&page.size={pageSize}";
        return await GetAsync<PaginatedResponse<BuildingAssessmentItemResponse>>(
            $"api/buildings/{buildingId}/assessments{qs}", cancellationToken).ConfigureAwait(false);
    }
}
