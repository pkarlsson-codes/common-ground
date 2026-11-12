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
    IHttpClientFactory httpClientFactory,
    ILogger<HomesApiClient> logger) : BaseApiClient(httpClientFactory.CreateClient(nameof(HomesApiClient)), logger), IHomesApiClient
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
        GetHomesQuerySpec querySpec, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(querySpec);

        return await GetAsync<PaginatedResponse<HomeListItemResponse>>(
            $"api/homes{querySpec.ToQueryString()}", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task ReleaseHomeForSaleAsync(string homeId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);
        await PostAsync($"api/homes/{homeId}/release-for-sale", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task SetHomeIncomeLimitAsync(string homeId, SetHomeIncomeLimitRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);
        ArgumentNullException.ThrowIfNull(request);
        await PutAsync($"api/homes/{homeId}/income-limit", request, cancellationToken).ConfigureAwait(false);
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
        string homeId, GetHomeAssessmentsQuerySpec querySpec, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);
        ArgumentNullException.ThrowIfNull(querySpec);
        return await GetAsync<PaginatedResponse<HomeAssessmentItemResponse>>(
            $"api/homes/{homeId}/assessments{querySpec.ToQueryString()}", cancellationToken).ConfigureAwait(false);
    }
}
