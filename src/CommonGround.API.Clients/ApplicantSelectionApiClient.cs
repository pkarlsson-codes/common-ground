using CommonGround.API.Contracts.ApplicantSelection.Requests;
using CommonGround.API.Contracts.ApplicantSelection.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.Extensions.Logging;

namespace CommonGround.API.Clients;

/// <summary>HTTP client for Applicant Selection homeowner application endpoints.</summary>
public sealed class ApplicantSelectionApiClient(
    HttpClient httpClient,
    ILogger<ApplicantSelectionApiClient> logger) : BaseApiClient(httpClient, logger), IApplicantSelectionApiClient
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
    public async Task ApproveEligibilityAsync(string applicationId, ApproveEligibilityRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/homeowner-applications/{applicationId}/eligibility/approve", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task DenyEligibilityAsync(string applicationId, DenyEligibilityRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/homeowner-applications/{applicationId}/eligibility/deny", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task RecordHousingCounselingAsync(string applicationId, RecordHousingCounselingRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/homeowner-applications/{applicationId}/housing-counseling", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task RecordCltOrientationAsync(string applicationId, RecordCltOrientationRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/homeowner-applications/{applicationId}/clt-orientation", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task RecordMortgagePrequalificationAsync(string applicationId, RecordMortgagePrequalificationRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/homeowner-applications/{applicationId}/mortgage-prequalification", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<HomeCandidateResponse>> GetHomeCandidatesAsync(string homeId, string? search, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);
        var query = string.IsNullOrWhiteSpace(search) ? string.Empty : $"?search={Uri.EscapeDataString(search)}";
        return await GetAsync<IReadOnlyList<HomeCandidateResponse>>($"api/homes/{homeId}/applicant-candidates{query}", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task SelectApplicantAsync(string applicationId, SelectApplicantRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/homeowner-applications/{applicationId}/select", request, cancellationToken).ConfigureAwait(false);
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