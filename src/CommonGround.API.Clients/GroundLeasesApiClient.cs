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
    public async Task<WaiveLeaseFeeInterestResponse> WaiveLeaseFeeInterestAsync(string groundLeaseId, WaiveLeaseFeeInterestRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);
        return await PostAsync<WaiveLeaseFeeInterestRequest, WaiveLeaseFeeInterestResponse>($"api/ground-leases/{groundLeaseId}/lease-fee-interest-waivers", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task CorrectLeaseFeeTrackingAsync(string groundLeaseId, CorrectLeaseFeeTrackingRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);
        await PutAsync($"api/ground-leases/{groundLeaseId}/lease-fee-tracking", request, cancellationToken).ConfigureAwait(false);
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

    /// <inheritdoc/>
    public async Task<RecordPermittedMortgageResponse> RecordPermittedMortgageAsync(string groundLeaseId, RecordPermittedMortgageRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);
        return await PostAsync<RecordPermittedMortgageRequest, RecordPermittedMortgageResponse>($"api/ground-leases/{groundLeaseId}/permitted-mortgages", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task EditPermittedMortgageAsync(string groundLeaseId, string mortgageId, EditPermittedMortgageRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentException.ThrowIfNullOrWhiteSpace(mortgageId);
        ArgumentNullException.ThrowIfNull(request);
        await PutAsync($"api/ground-leases/{groundLeaseId}/permitted-mortgages/{mortgageId}", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task MarkPermittedMortgagePaidOffAsync(string groundLeaseId, string mortgageId, MarkPermittedMortgagePaidOffRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentException.ThrowIfNullOrWhiteSpace(mortgageId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/ground-leases/{groundLeaseId}/permitted-mortgages/{mortgageId}/paid-off", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task SetInsuranceRequirementsAsync(string groundLeaseId, SetInsuranceRequirementsRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/ground-leases/{groundLeaseId}/insurance-requirements", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<RecordInsuranceCertificateResponse> RecordInsuranceCertificateAsync(string groundLeaseId, RecordInsuranceCertificateRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);
        return await PostAsync<RecordInsuranceCertificateRequest, RecordInsuranceCertificateResponse>($"api/ground-leases/{groundLeaseId}/insurance-certificates", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task RemoveInsuranceCertificateAsync(string groundLeaseId, string certificateId, RemoveInsuranceCertificateRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentException.ThrowIfNullOrWhiteSpace(certificateId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/ground-leases/{groundLeaseId}/insurance-certificates/{certificateId}/remove", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<ReceiveMortgageRequestResponse> ReceiveMortgageRequestAsync(string groundLeaseId, ReceiveMortgageRequestRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);
        return await PostAsync<ReceiveMortgageRequestRequest, ReceiveMortgageRequestResponse>($"api/ground-leases/{groundLeaseId}/mortgage-requests", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<MortgageRequestResponse>> GetMortgageRequestsAsync(string groundLeaseId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        return await GetAsync<IReadOnlyList<MortgageRequestResponse>>($"api/ground-leases/{groundLeaseId}/mortgage-requests", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<MortgageRequestResponse> GetMortgageRequestAsync(string mortgageRequestId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mortgageRequestId);
        return await GetAsync<MortgageRequestResponse>($"api/mortgage-requests/{mortgageRequestId}", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task RecordMortgageRequestAppraisalAsync(string mortgageRequestId, RecordMortgageRequestAppraisalRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mortgageRequestId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/mortgage-requests/{mortgageRequestId}/appraisal", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task ApproveMortgageRequestAsync(string mortgageRequestId, ApproveMortgageRequestRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mortgageRequestId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/mortgage-requests/{mortgageRequestId}/approve", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task DenyMortgageRequestAsync(string mortgageRequestId, DenyMortgageRequestRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mortgageRequestId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/mortgage-requests/{mortgageRequestId}/deny", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<RecordMortgageLoanClosedResponse> RecordMortgageLoanClosedAsync(string mortgageRequestId, RecordMortgageLoanClosedRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mortgageRequestId);
        ArgumentNullException.ThrowIfNull(request);
        return await PostAsync<RecordMortgageLoanClosedRequest, RecordMortgageLoanClosedResponse>($"api/mortgage-requests/{mortgageRequestId}/loan-closed", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task WithdrawMortgageRequestAsync(string mortgageRequestId, WithdrawMortgageRequestRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mortgageRequestId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/mortgage-requests/{mortgageRequestId}/withdraw", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<ReceiveImprovementRequestResponse> ReceiveImprovementRequestAsync(string groundLeaseId, ReceiveImprovementRequestRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);
        return await PostAsync<ReceiveImprovementRequestRequest, ReceiveImprovementRequestResponse>($"api/ground-leases/{groundLeaseId}/improvement-requests", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ImprovementRequestResponse>> GetImprovementRequestsAsync(string groundLeaseId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        return await GetAsync<IReadOnlyList<ImprovementRequestResponse>>($"api/ground-leases/{groundLeaseId}/improvement-requests", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<ImprovementRequestResponse> GetImprovementRequestAsync(string improvementRequestId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(improvementRequestId);
        return await GetAsync<ImprovementRequestResponse>($"api/improvement-requests/{improvementRequestId}", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task ApproveImprovementRequestAsync(string improvementRequestId, ApproveImprovementRequestRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(improvementRequestId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/improvement-requests/{improvementRequestId}/approve", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task DenyImprovementRequestAsync(string improvementRequestId, DenyImprovementRequestRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(improvementRequestId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/improvement-requests/{improvementRequestId}/deny", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task RecordImprovementCompletedAsync(string improvementRequestId, RecordImprovementCompletedRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(improvementRequestId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/improvement-requests/{improvementRequestId}/completed", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task WithdrawImprovementRequestAsync(string improvementRequestId, WithdrawImprovementRequestRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(improvementRequestId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/improvement-requests/{improvementRequestId}/withdraw", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<ResalePriceStatementPriceResponse> PreviewResalePriceStatementAsync(string groundLeaseId, PreviewResalePriceStatementRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);
        return await PostAsync<PreviewResalePriceStatementRequest, ResalePriceStatementPriceResponse>($"api/ground-leases/{groundLeaseId}/resale-price-statements/preview", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task RecordResalePriceStatementAsync(string groundLeaseId, RecordResalePriceStatementRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/ground-leases/{groundLeaseId}/resale-price-statements", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<ResalePriceStatementsDueResponse> GetResalePriceStatementsDueAsync(bool includeAll, string? search, CancellationToken cancellationToken)
    {
        var query = $"?includeAll={(includeAll ? "true" : "false")}";
        if (!string.IsNullOrWhiteSpace(search))
        {
            query += $"&search={Uri.EscapeDataString(search)}";
        }

        return await GetAsync<ResalePriceStatementsDueResponse>($"api/resale-price-statements/due{query}", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<ResalePriceStatementPreviewsResponse> PreviewResalePriceStatementsAsync(PreviewResalePriceStatementsRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await PostAsync<PreviewResalePriceStatementsRequest, ResalePriceStatementPreviewsResponse>("api/resale-price-statements/preview", request, cancellationToken).ConfigureAwait(false);
    }
}
