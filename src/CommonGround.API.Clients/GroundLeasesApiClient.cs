using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.Extensions.Logging;

namespace CommonGround.API.Clients;

/// <summary>HTTP client for ground lease API endpoints.</summary>
public sealed class GroundLeasesApiClient(
    IHttpClientFactory httpClientFactory,
    ILogger<GroundLeasesApiClient> logger) : BaseApiClient(httpClientFactory.CreateClient(nameof(GroundLeasesApiClient)), logger), IGroundLeasesApiClient
{
    /// <inheritdoc/>
    public async Task<CreateGroundLeaseResponse> CreateGroundLeaseAsync(CreateGroundLeaseRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await PostAsync<CreateGroundLeaseRequest, CreateGroundLeaseResponse>("api/ground-leases", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<GroundLeaseResponse> GetGroundLeaseAsync(string groundLeaseId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        return await GetAsync<GroundLeaseResponse>($"api/ground-leases/{groundLeaseId}", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<PaginatedResponse<GroundLeaseListItemResponse>> GetGroundLeasesAsync(
        GetGroundLeasesQuerySpec querySpec, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(querySpec);

        return await GetAsync<PaginatedResponse<GroundLeaseListItemResponse>>($"api/ground-leases{querySpec.ToQueryString()}", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task RecordStewardshipCheckInAsync(string groundLeaseId, RecordStewardshipCheckInRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/ground-leases/{groundLeaseId}/stewardship-check-ins", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<ResaleEstimateResponse> CalculateResaleEstimateAsync(string groundLeaseId, CalculateResaleEstimateRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);
        return await PostAsync<CalculateResaleEstimateRequest, ResaleEstimateResponse>($"api/ground-leases/{groundLeaseId}/resale-estimate", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<RecordLeaseFeePaymentResponse> RecordLeaseFeePaymentAsync(string groundLeaseId, RecordLeaseFeePaymentRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);
        return await PostAsync<RecordLeaseFeePaymentRequest, RecordLeaseFeePaymentResponse>($"api/ground-leases/{groundLeaseId}/lease-fee-payments", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task VoidLeaseFeePaymentAsync(string groundLeaseId, string paymentId, VoidLeaseFeePaymentRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentException.ThrowIfNullOrWhiteSpace(paymentId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/ground-leases/{groundLeaseId}/lease-fee-payments/{paymentId}/void", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<PaginatedResponse<LeaseFeeBalanceResponse>> GetLeaseFeeBalancesAsync(GetLeaseFeeBalancesQuerySpec querySpec, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(querySpec);
        return await GetAsync<PaginatedResponse<LeaseFeeBalanceResponse>>($"api/lease-fee-balances{querySpec.ToQueryString()}", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<RecordLeaseFeeNoticeResponse> RecordLeaseFeeNoticeAsync(string groundLeaseId, RecordLeaseFeeNoticeRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);
        return await PostAsync<RecordLeaseFeeNoticeRequest, RecordLeaseFeeNoticeResponse>($"api/ground-leases/{groundLeaseId}/lease-fee-notices", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<LeaseFeeNoticesDueResponse> GetLeaseFeeNoticesDueAsync(string? groundLeaseId, CancellationToken cancellationToken)
    {
        var query = string.IsNullOrWhiteSpace(groundLeaseId) ? string.Empty : $"?groundLeaseId={Uri.EscapeDataString(groundLeaseId)}";
        return await GetAsync<LeaseFeeNoticesDueResponse>($"api/lease-fee-notices/due{query}", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<LeaseFeeNoticePolicyResponse> GetLeaseFeeNoticePolicyAsync(CancellationToken cancellationToken) =>
        await GetAsync<LeaseFeeNoticePolicyResponse>("api/lease-fee-notice-policy", cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<LeaseFeeNoticePolicyResponse> SetLeaseFeeNoticePolicyAsync(SetLeaseFeeNoticePolicyRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await PutAsync<SetLeaseFeeNoticePolicyRequest, LeaseFeeNoticePolicyResponse>("api/lease-fee-notice-policy", request, cancellationToken).ConfigureAwait(false);
    }
}
