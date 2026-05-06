using CommonGround.API.Contracts.PropertyRegistry.Requests;
using CommonGround.API.Contracts.PropertyRegistry.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.Extensions.Logging;

namespace CommonGround.API.Clients;

/// <summary>
/// HTTP client for home API endpoints.
/// All requests are automatically decorated with an internal JWT by <c>InternalGatewayAuthHandler</c>.
/// </summary>
public sealed class HomesApiClient(
    HttpClient httpClient,
    ILogger<HomesApiClient> logger) : BaseApiClient(httpClient, logger), IHomesApiClient
{
    /// <inheritdoc/>
    public async Task<RegisterHomeResponse> RegisterHomeAsync(RegisterHomeRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await PostAsync<RegisterHomeRequest, RegisterHomeResponse>("api/homes", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<HomeResponse> GetHomeAsync(string homeId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);
        return await GetAsync<HomeResponse>($"api/homes/{homeId}", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<PaginatedResponse<HomeListItemResponse>> GetHomesAsync(
        int pageNumber, int pageSize, string? landParcelId, string? status, CancellationToken cancellationToken)
    {
        var qs = $"?pageNumber={pageNumber}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(landParcelId))
        {
            qs += $"&landParcelIds={Uri.EscapeDataString(landParcelId)}";
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            qs += $"&statuses={Uri.EscapeDataString(status)}";
        }

        return await GetAsync<PaginatedResponse<HomeListItemResponse>>($"api/homes{qs}", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task RecordHomeAssessmentAsync(string homeId, RecordHomeAssessmentRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/homes/{homeId}/assessments", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<PaginatedResponse<HomeAssessmentItemResponse>> GetHomeAssessmentsAsync(
        string homeId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);
        var qs = $"?pageNumber={pageNumber}&pageSize={pageSize}";
        return await GetAsync<PaginatedResponse<HomeAssessmentItemResponse>>(
            $"api/homes/{homeId}/assessments{qs}", cancellationToken).ConfigureAwait(false);
    }
}
