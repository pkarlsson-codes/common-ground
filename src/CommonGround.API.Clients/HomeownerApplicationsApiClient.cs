using CommonGround.API.Contracts.HomeBuyers.Requests;
using CommonGround.API.Contracts.HomeBuyers.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.Extensions.Logging;

namespace CommonGround.API.Clients;

/// <summary>HTTP client for homeowner application endpoints.</summary>
public sealed class HomeownerApplicationsApiClient(
    HttpClient httpClient,
    ILogger<HomeownerApplicationsApiClient> logger) : BaseApiClient(httpClient, logger), IHomeownerApplicationsApiClient
{
    /// <inheritdoc/>
    public async Task<SubmitApplicationResponse> SubmitApplicationAsync(SubmitApplicationRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await PostAsync<SubmitApplicationRequest, SubmitApplicationResponse>("api/homeowner-applications", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<PaginatedResponse<HomeownerApplicationListItemResponse>> GetHomeownerApplicationsAsync(
        GetHomeownerApplicationsQuerySpec querySpec,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(querySpec);
        return await GetAsync<PaginatedResponse<HomeownerApplicationListItemResponse>>(
            $"api/homeowner-applications{querySpec.ToQueryString()}", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<HomeownerApplicationResponse> GetHomeownerApplicationAsync(string applicationId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        return await GetAsync<HomeownerApplicationResponse>($"api/homeowner-applications/{applicationId}", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task ApproveApplicationAsync(string applicationId, ApproveApplicationRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/homeowner-applications/{applicationId}/approve", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<PaginatedResponse<WaitingListEntryResponse>> GetWaitingListAsync(
        GetWaitingListQuerySpec querySpec,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(querySpec);
        return await GetAsync<PaginatedResponse<WaitingListEntryResponse>>(
            $"api/waiting-list{querySpec.ToQueryString()}", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task DenyApplicationAsync(string applicationId, DenyApplicationRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/homeowner-applications/{applicationId}/deny", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task WithdrawApplicationAsync(string applicationId, WithdrawApplicationRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/homeowner-applications/{applicationId}/withdraw", request, cancellationToken).ConfigureAwait(false);
    }
}