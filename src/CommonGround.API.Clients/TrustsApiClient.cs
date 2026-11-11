using CommonGround.API.Contracts.TrustManagement.Requests;
using CommonGround.API.Contracts.TrustManagement.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.Extensions.Logging;

namespace CommonGround.API.Clients;

/// <summary>
/// HTTP client for trust management API endpoints.
/// </summary>
public sealed class TrustsApiClient(
    IHttpClientFactory httpClientFactory,
    ILogger<TrustsApiClient> logger) : BaseApiClient(httpClientFactory.CreateClient(nameof(TrustsApiClient)), logger), ITrustsApiClient
{
    /// <inheritdoc/>
    public async Task<TrustResponse> GetCurrentTrustAsync(CancellationToken ct = default)
        => await GetAsync<TrustResponse>("api/trusts/current", ct).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<CreateTrustResponse> CreateTrustAsync(CreateTrustRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await PostAsync<CreateTrustRequest, CreateTrustResponse>("api/trusts", request, ct).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<CreateTrustInvitationResponse> CreateTrustInvitationAsync(CreateTrustInvitationRequest request, CancellationToken ct = default)
        => await PostAsync<CreateTrustInvitationRequest, CreateTrustInvitationResponse>("api/trust-invitations", request, ct).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<TrustInvitationListResponse> ListTrustInvitationsAsync(CancellationToken ct = default)
        => await GetAsync<TrustInvitationListResponse>("api/trust-invitations", ct).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task CancelTrustInvitationAsync(string invitationId, CancellationToken ct = default)
        => await PostAsync($"api/trust-invitations/{Uri.EscapeDataString(invitationId)}/cancel", ct).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<TrustInvitationPreviewResponse> PreviewTrustInvitationAsync(TrustInvitationTokenRequest request, CancellationToken ct = default)
        => await PostAsync<TrustInvitationTokenRequest, TrustInvitationPreviewResponse>("api/trust-invitations/preview", request, ct).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<AcceptTrustInvitationResponse> AcceptTrustInvitationAsync(TrustInvitationTokenRequest request, CancellationToken ct = default)
        => await PostAsync<TrustInvitationTokenRequest, AcceptTrustInvitationResponse>("api/trust-invitations/accept", request, ct).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<TrustMemberListResponse> ListTrustMembersAsync(CancellationToken ct = default)
        => await GetAsync<TrustMemberListResponse>("api/trust-members", ct).ConfigureAwait(false);
}
