using CommonGround.API.Contracts.ApplicantSelection.Requests;
using CommonGround.API.Contracts.ApplicantSelection.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.Extensions.Logging;

namespace CommonGround.API.Clients;

/// <summary>HTTP client for household endpoints.</summary>
public sealed class HouseholdsApiClient(
    HttpClient httpClient,
    ILogger<HouseholdsApiClient> logger) : BaseApiClient(httpClient, logger), IHouseholdsApiClient
{
    /// <inheritdoc/>
    public async Task<RegisterHouseholdResponse> RegisterHouseholdAsync(RegisterHouseholdRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await PostAsync<RegisterHouseholdRequest, RegisterHouseholdResponse>("api/households", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<HouseholdResponse> GetHouseholdAsync(string householdId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(householdId);
        return await GetAsync<HouseholdResponse>($"api/households/{householdId}", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task RecordHousingCounselingAsync(string householdId, RecordHousingCounselingRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(householdId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/households/{householdId}/housing-counseling", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task RecordCltOrientationAsync(string householdId, RecordCltOrientationRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(householdId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/households/{householdId}/clt-orientation", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task RecordMortgagePrequalificationAsync(string householdId, RecordMortgagePrequalificationRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(householdId);
        ArgumentNullException.ThrowIfNull(request);
        await PostAsync($"api/households/{householdId}/mortgage-prequalification", request, cancellationToken).ConfigureAwait(false);
    }
}
