using CommonGround.API.Contracts.Today.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.Extensions.Logging;

namespace CommonGround.API.Clients;

/// <summary>HTTP client for the Today API endpoint.</summary>
public sealed class TodayApiClient(
    IHttpClientFactory httpClientFactory,
    ILogger<TodayApiClient> logger) : BaseApiClient(httpClientFactory.CreateClient(nameof(TodayApiClient)), logger), ITodayApiClient
{
    /// <inheritdoc/>
    public async Task<TodayResponse> GetTodayAsync(IReadOnlyList<string>? areas, CancellationToken ct = default)
        => await GetAsync<TodayResponse>($"api/today{new QueryStringBuilder().AddEach("areas", areas).Build()}", ct).ConfigureAwait(false);
}
