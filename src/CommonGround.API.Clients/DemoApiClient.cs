using CommonGround.API.Contracts.Demo.Requests;
using CommonGround.API.Contracts.Demo.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.Extensions.Logging;

namespace CommonGround.API.Clients;

/// <summary>HTTP client for the demo mode API endpoints.</summary>
public sealed class DemoApiClient(
    IHttpClientFactory httpClientFactory,
    ILogger<DemoApiClient> logger) : BaseApiClient(httpClientFactory.CreateClient(nameof(DemoApiClient)), logger), IDemoApiClient
{
    /// <inheritdoc/>
    public async Task<DemoModeResponse> GetModeAsync(CancellationToken ct = default)
        => await GetAsync<DemoModeResponse>("api/demo/mode", ct).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<DemoResponse> GetDemoAsync(CancellationToken ct = default)
        => await GetAsync<DemoResponse>("api/demo", ct).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task SendFeedbackAsync(SendDemoFeedbackRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync("api/demo/feedback", request, ct).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<DemoSandboxResponse> StartSandboxAsync(CancellationToken ct = default)
        => await PostAsync<DemoSandboxResponse>("api/demo/sandboxes", ct).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<CurrentDemoSandboxResponse> GetCurrentSandboxAsync(CancellationToken ct = default)
        => await GetAsync<CurrentDemoSandboxResponse>("api/demo/sandboxes/current", ct).ConfigureAwait(false);
}
