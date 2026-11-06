using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.Extensions.Logging;

namespace CommonGround.API.Clients;

/// <summary>HTTP client for resale API endpoints.</summary>
public sealed class ResalesApiClient(
    IHttpClientFactory httpClientFactory,
    ILogger<ResalesApiClient> logger) : BaseApiClient(httpClientFactory.CreateClient(nameof(ResalesApiClient)), logger), IResalesApiClient
{
    /// <inheritdoc/>
    public async Task<RecordIntentToSellResponse> RecordIntentToSellAsync(string groundLeaseId, RecordIntentToSellRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);
        return await PostAsync<RecordIntentToSellRequest, RecordIntentToSellResponse>($"api/ground-leases/{groundLeaseId}/intent-to-sell", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task WithdrawIntentToSellAsync(string resaleId, WithdrawIntentToSellRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resaleId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/resales/{resaleId}/withdraw", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<PaginatedResponse<ResaleResponse>> GetResalesAsync(GetResalesQuerySpec querySpec, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(querySpec);
        return await GetAsync<PaginatedResponse<ResaleResponse>>($"api/resales{querySpec.ToQueryString()}", cancellationToken).ConfigureAwait(false);
    }
}
