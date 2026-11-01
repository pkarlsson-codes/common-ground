using CommonGround.API.Contracts.Stewardship.Requests;
using CommonGround.API.Contracts.Stewardship.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.Extensions.Logging;

namespace CommonGround.API.Clients;

/// <summary>HTTP client for Stewardship API endpoints.</summary>
public sealed class StewardshipApiClient(
    IHttpClientFactory httpClientFactory,
    ILogger<StewardshipApiClient> logger) : BaseApiClient(httpClientFactory.CreateClient(nameof(StewardshipApiClient)), logger), IStewardshipApiClient
{
    // Stewardship Cases

    /// <inheritdoc/>
    public async Task<StewardshipCaseResponse> GetStewardshipCaseAsync(string caseId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        return await GetAsync<StewardshipCaseResponse>($"api/stewardship/cases/{caseId}", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public Task<PaginatedResponse<StewardshipCaseResponse>> GetStewardshipCasesAsync(GetStewardshipCasesQuerySpec querySpec, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(querySpec);
        return GetAsync<PaginatedResponse<StewardshipCaseResponse>>($"api/stewardship/cases{querySpec.ToQueryString()}", cancellationToken);
    }

    /// <inheritdoc/>
    public async Task InitiateOccupancyVerificationAsync(string caseId, InitiateOccupancyVerificationRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/stewardship/cases/{caseId}/occupancy-verifications", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task SubmitOccupancyVerificationAsync(string caseId, Guid verificationId, SubmitOccupancyVerificationRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/stewardship/cases/{caseId}/occupancy-verifications/{verificationId}/submit", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task VerifyOccupancyAsync(string caseId, Guid verificationId, VerifyOccupancyRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/stewardship/cases/{caseId}/occupancy-verifications/{verificationId}/verify", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task FailOccupancyAsync(string caseId, Guid verificationId, FailOccupancyRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/stewardship/cases/{caseId}/occupancy-verifications/{verificationId}/fail", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task MarkOccupancyVerificationOverdueAsync(string caseId, Guid verificationId, MarkOccupancyVerificationOverdueRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/stewardship/cases/{caseId}/occupancy-verifications/{verificationId}/overdue", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task SchedulePropertyInspectionAsync(string caseId, SchedulePropertyInspectionRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/stewardship/cases/{caseId}/inspections", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task CompletePropertyInspectionAsync(string caseId, Guid inspectionId, CompletePropertyInspectionRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/stewardship/cases/{caseId}/inspections/{inspectionId}/complete", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task CancelPropertyInspectionAsync(string caseId, Guid inspectionId, CancelPropertyInspectionRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/stewardship/cases/{caseId}/inspections/{inspectionId}/cancel", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task IdentifyViolationAsync(string caseId, IdentifyViolationRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/stewardship/cases/{caseId}/violations", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task IssueViolationNoticeAsync(string caseId, Guid violationId, IssueViolationNoticeRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/stewardship/cases/{caseId}/violations/{violationId}/notice", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task CureViolationAsync(string caseId, Guid violationId, CureViolationRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/stewardship/cases/{caseId}/violations/{violationId}/cure", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task EscalateViolationAsync(string caseId, Guid violationId, EscalateViolationRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/stewardship/cases/{caseId}/violations/{violationId}/escalate", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task CloseStewardshipCaseAsync(string caseId, CloseStewardshipCaseRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/stewardship/cases/{caseId}/close", request, cancellationToken).ConfigureAwait(false);
    }

    // Foreclosure Prevention Cases

    /// <inheritdoc/>
    public async Task<OpenForeclosurePreventionCaseResponse> OpenForeclosurePreventionCaseAsync(OpenForeclosurePreventionCaseRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await PostAsync<OpenForeclosurePreventionCaseRequest, OpenForeclosurePreventionCaseResponse>(
            "api/stewardship/foreclosure-prevention", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<ForeclosurePreventionCaseResponse> GetForeclosurePreventionCaseAsync(string caseId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        return await GetAsync<ForeclosurePreventionCaseResponse>($"api/stewardship/foreclosure-prevention/{caseId}", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public Task<PaginatedResponse<ForeclosurePreventionCaseResponse>> GetForeclosurePreventionCasesAsync(GetForeclosurePreventionCasesQuerySpec querySpec, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(querySpec);
        return GetAsync<PaginatedResponse<ForeclosurePreventionCaseResponse>>($"api/stewardship/foreclosure-prevention{querySpec.ToQueryString()}", cancellationToken);
    }

    /// <inheritdoc/>
    public async Task RecordServicerDelinquencyAsync(string caseId, RecordServicerDelinquencyRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/stewardship/foreclosure-prevention/{caseId}/servicer-delinquency", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task MakeCounselingReferralAsync(string caseId, MakeCounselingReferralRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/stewardship/foreclosure-prevention/{caseId}/counseling-referrals", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task RecordCounselingOutcomeAsync(string caseId, Guid referralId, RecordCounselingOutcomeRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/stewardship/foreclosure-prevention/{caseId}/counseling-referrals/{referralId}/outcome", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task RequestForbearanceAsync(string caseId, RequestForbearanceRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/stewardship/foreclosure-prevention/{caseId}/forbearance-requests", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task RecordForbearanceOutcomeAsync(string caseId, Guid requestId, RecordForbearanceOutcomeRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/stewardship/foreclosure-prevention/{caseId}/forbearance-requests/{requestId}/outcome", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task ActivateCurePeriodAsync(string caseId, ActivateCurePeriodRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/stewardship/foreclosure-prevention/{caseId}/cure-period", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task ResolveForeclosurePreventionCaseAsync(string caseId, ResolveForeclosurePreventionCaseRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/stewardship/foreclosure-prevention/{caseId}/resolve", request, cancellationToken).ConfigureAwait(false);
    }
}
