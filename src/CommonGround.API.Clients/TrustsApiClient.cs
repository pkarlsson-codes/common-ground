using CommonGround.API.Contracts.TrustManagement.Requests;
using CommonGround.API.Contracts.TrustManagement.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.Extensions.Logging;

namespace CommonGround.API.Clients;

/// <summary>
/// HTTP client for trust management API endpoints.
/// </summary>
public sealed class TrustsApiClient(
    IHttpClientFactory httpClientFactory,
    ILogger<TrustsApiClient> logger) : BaseApiClient(httpClientFactory.CreateClient(nameof(TrustsApiClient)), logger), ITrustsApiClient
{
    /// <inheritdoc/>
    public async Task<TrustResponse> GetCurrentTrustAsync(CancellationToken ct = default)
        => await GetAsync<TrustResponse>("api/trusts/current", ct).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<CreateTrustResponse> CreateTrustAsync(CreateTrustRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await PostAsync<CreateTrustRequest, CreateTrustResponse>("api/trusts", request, ct).ConfigureAwait(false);
    }
}
