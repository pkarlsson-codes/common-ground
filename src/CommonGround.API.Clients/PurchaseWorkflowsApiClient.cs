using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.Extensions.Logging;

namespace CommonGround.API.Clients;

/// <summary>HTTP client for purchase workflow API endpoints.</summary>
public sealed class PurchaseWorkflowsApiClient(
    HttpClient httpClient,
    ILogger<PurchaseWorkflowsApiClient> logger) : BaseApiClient(httpClient, logger), IPurchaseWorkflowsApiClient
{
    /// <inheritdoc/>
    public async Task<InitiatePurchaseWorkflowResponse> InitiatePurchaseWorkflowAsync(InitiatePurchaseWorkflowRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await PostAsync<InitiatePurchaseWorkflowRequest, InitiatePurchaseWorkflowResponse>("api/purchase-workflows", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<PurchaseWorkflowResponse> GetPurchaseWorkflowAsync(string workflowId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        return await GetAsync<PurchaseWorkflowResponse>($"api/purchase-workflows/{workflowId}", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<PaginatedResponse<PurchaseWorkflowListItemResponse>> GetPurchaseWorkflowsAsync(
        int pageNumber, int pageSize, string? groundLeaseTemplateId, string? phase, string? search, CancellationToken cancellationToken)
    {
        var qs = $"?pageNumber={pageNumber}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(groundLeaseTemplateId))
        {
            qs += $"&groundLeaseId={Uri.EscapeDataString(groundLeaseTemplateId)}";
        }

        if (!string.IsNullOrWhiteSpace(phase))
        {
            qs += $"&phase={Uri.EscapeDataString(phase)}";
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            qs += $"&search={Uri.EscapeDataString(search)}";
        }

        return await GetAsync<PaginatedResponse<PurchaseWorkflowListItemResponse>>($"api/purchase-workflows{qs}", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task RecordEligibilityVerifiedAsync(string workflowId, RecordEligibilityVerifiedRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        await PostAsync($"api/purchase-workflows/{workflowId}/eligibility", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task RecordFinancingCommittedAsync(string workflowId, RecordFinancingCommittedRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        await PostAsync($"api/purchase-workflows/{workflowId}/financing", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task RecordInspectionCompletedAsync(string workflowId, RecordInspectionCompletedRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        await PostAsync($"api/purchase-workflows/{workflowId}/inspection", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task RecordTitleClearedAsync(string workflowId, RecordTitleClearedRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        await PostAsync($"api/purchase-workflows/{workflowId}/title", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task RecordLeaseExecutedAsync(string workflowId, RecordLeaseExecutedRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        await PostAsync($"api/purchase-workflows/{workflowId}/lease-execution", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task ScheduleClosingAsync(string workflowId, ScheduleClosingRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        await PostAsync($"api/purchase-workflows/{workflowId}/closing/schedule", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task RecordPurchaseClosedAsync(string workflowId, RecordPurchaseClosedRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        await PostAsync($"api/purchase-workflows/{workflowId}/closing/record", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task FailPurchaseAsync(string workflowId, FailPurchaseRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        await PostAsync($"api/purchase-workflows/{workflowId}/fail", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task WithdrawPurchaseAsync(string workflowId, WithdrawPurchaseRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        await PostAsync($"api/purchase-workflows/{workflowId}/withdraw", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task StallWorkflowAsync(string workflowId, StallWorkflowRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        await PostAsync($"api/purchase-workflows/{workflowId}/stall", request, cancellationToken).ConfigureAwait(false);
    }
}
