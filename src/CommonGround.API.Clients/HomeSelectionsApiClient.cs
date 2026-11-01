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
    public async Task<StartHomeSelectionResponse> StartHomeSelectionAsync(string homeId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);
        return await PostAsync<StartHomeSelectionResponse>($"api/homes/{homeId}/home-selections", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<HomeSelectionSummaryResponse>> GetHomeSelectionsForHomeAsync(string homeId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);
        return await GetAsync<IReadOnlyList<HomeSelectionSummaryResponse>>($"api/homes/{homeId}/home-selections", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<PaginatedResponse<HomeSelectionListItemResponse>> GetHomeSelectionsAsync(
        GetHomeSelectionsQuerySpec querySpec, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(querySpec);
        return await GetAsync<PaginatedResponse<HomeSelectionListItemResponse>>($"api/home-selections{querySpec.ToQueryString()}", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<HomeSelectionResponse> GetHomeSelectionAsync(string selectionId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(selectionId);
        return await GetAsync<HomeSelectionResponse>($"api/home-selections/{selectionId}", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task SelectHouseholdByPriorityAsync(string selectionId, SelectHouseholdByPriorityRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(selectionId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/home-selections/{selectionId}/select-by-priority", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task DrawSelectionLotteryAsync(string selectionId, DrawSelectionLotteryRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(selectionId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/home-selections/{selectionId}/draw-lottery", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task CancelHomeSelectionAsync(string selectionId, CancelHomeSelectionRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(selectionId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/home-selections/{selectionId}/cancel", request, cancellationToken).ConfigureAwait(false);
    }
}
