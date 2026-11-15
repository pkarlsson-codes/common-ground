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

    /// <inheritdoc/>
    public async Task<ResaleResponse> GetResaleAsync(string resaleId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resaleId);
        return await GetAsync<ResaleResponse>($"api/resales/{resaleId}", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<ResalePriceResponse> PreviewResalePriceAsync(string resaleId, PreviewResalePriceRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resaleId);
        ArgumentNullException.ThrowIfNull(request);
        return await PostAsync<PreviewResalePriceRequest, ResalePriceResponse>($"api/resales/{resaleId}/price-preview", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task RecordResaleAppraisalAsync(string resaleId, RecordResaleAppraisalRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resaleId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/resales/{resaleId}/appraisal", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task ExercisePurchaseOptionAsync(string resaleId, ExercisePurchaseOptionRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resaleId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/resales/{resaleId}/exercise-option", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task RecordBuybackClosedAsync(string resaleId, RecordBuybackClosedRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resaleId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/resales/{resaleId}/buyback-closing", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<PayoffStatementResponse> GetPayoffStatementAsync(string resaleId, DateOnly goodThrough, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resaleId);
        return await GetAsync<PayoffStatementResponse>($"api/resales/{resaleId}/payoff-statement?goodThrough={goodThrough:yyyy-MM-dd}", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task RecordPayoffStatementAsync(string resaleId, RecordPayoffStatementRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resaleId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/resales/{resaleId}/payoff-statements", request, cancellationToken).ConfigureAwait(false);
    }
}
