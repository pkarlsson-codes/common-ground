using CommonGround.API.Contracts.HomeBuyers.Requests;
using CommonGround.API.Contracts.HomeBuyers.Responses;
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
    public async Task<PaginatedResponse<HouseholdListItemResponse>> GetHouseholdsAsync(
        GetHouseholdsQuerySpec querySpec,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(querySpec);
        return await GetAsync<PaginatedResponse<HouseholdListItemResponse>>(
            $"api/households{querySpec.ToQueryString()}", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<HouseholdResponse> GetHouseholdAsync(string householdId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(householdId);
        return await GetAsync<HouseholdResponse>($"api/households/{householdId}", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<AddHouseholdMemberResponse> AddHouseholdMemberAsync(string householdId, AddHouseholdMemberRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(householdId);
        ArgumentNullException.ThrowIfNull(request);
        return await PostAsync<AddHouseholdMemberRequest, AddHouseholdMemberResponse>($"api/households/{householdId}/members", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task UpdateHouseholdMemberAsync(string householdId, string memberId, UpdateHouseholdMemberRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(householdId);
        ArgumentException.ThrowIfNullOrWhiteSpace(memberId);
        ArgumentNullException.ThrowIfNull(request);
        await PutAsync($"api/households/{householdId}/members/{memberId}", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task RemoveHouseholdMemberAsync(string householdId, string memberId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(householdId);
        ArgumentException.ThrowIfNullOrWhiteSpace(memberId);
        await DeleteAsync($"api/households/{householdId}/members/{memberId}", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task UpdateHouseholdCircumstancesAsync(string householdId, UpdateHouseholdCircumstancesRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(householdId);
        ArgumentNullException.ThrowIfNull(request);
        await PutAsync($"api/households/{householdId}/circumstances", request, cancellationToken).ConfigureAwait(false);
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
