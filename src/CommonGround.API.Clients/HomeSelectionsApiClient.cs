using CommonGround.API.Contracts.HomeBuyers.Requests;
using CommonGround.API.Contracts.HomeBuyers.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.Extensions.Logging;

namespace CommonGround.API.Clients;

/// <summary>HTTP client for home selection endpoints.</summary>
public sealed class HomeSelectionsApiClient(
    IHttpClientFactory httpClientFactory,
    ILogger<HomeSelectionsApiClient> logger) : BaseApiClient(httpClientFactory.CreateClient(nameof(HomeSelectionsApiClient)), logger), IHomeSelectionsApiClient
{
    /// <inheritdoc/>
    public async Task<HomeSelectionOverviewResponse> GetHomeSelectionOverviewAsync(string homeId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);
        return await GetAsync<HomeSelectionOverviewResponse>($"api/homes/{homeId}/selection", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<PaginatedResponse<HomeSelectionListItemResponse>> GetHomeSelectionsAsync(
        GetHomeSelectionsQuerySpec querySpec, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(querySpec);
        return await GetAsync<PaginatedResponse<HomeSelectionListItemResponse>>($"api/home-selections{querySpec.ToQueryString()}", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<HomeSelectionCreatedResponse> SelectHouseholdByPriorityAsync(string homeId, SelectHouseholdByPriorityRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);
        ArgumentNullException.ThrowIfNull(request);
        return await PostAsync<SelectHouseholdByPriorityRequest, HomeSelectionCreatedResponse>(
            $"api/homes/{homeId}/home-selections/select-by-priority", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<HomeSelectionCreatedResponse> DrawSelectionLotteryAsync(string homeId, DrawSelectionLotteryRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);
        ArgumentNullException.ThrowIfNull(request);
        return await PostAsync<DrawSelectionLotteryRequest, HomeSelectionCreatedResponse>(
            $"api/homes/{homeId}/home-selections/draw-lottery", request, cancellationToken).ConfigureAwait(false);
    }
}
