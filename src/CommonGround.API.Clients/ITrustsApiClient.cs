using CommonGround.API.Contracts.TrustManagement.Requests;
using CommonGround.API.Contracts.TrustManagement.Responses;

namespace CommonGround.API.Clients;

/// <summary>HTTP client for the trust management API endpoints.</summary>
public interface ITrustsApiClient
{
    /// <summary>Gets the signed-in user's trust.</summary>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The trust, including its time zone.</returns>
    Task<TrustResponse> GetCurrentTrustAsync(CancellationToken ct = default);

    /// <summary>Creates a new trust and returns the response.</summary>
    /// <param name="request">The request containing the trust name.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The response containing the stable trust ID.</returns>
    Task<CreateTrustResponse> CreateTrustAsync(CreateTrustRequest request, CancellationToken ct = default);

    /// <summary>Makes an invite link to the caller's trust.</summary>
    /// <param name="request">The request.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The invitation and the token for its link.</returns>
    Task<CreateTrustInvitationResponse> CreateTrustInvitationAsync(CreateTrustInvitationRequest request, CancellationToken ct = default);

    /// <summary>Lists the caller's trust's invitations that can still be used.</summary>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The pending invitations.</returns>
    Task<TrustInvitationListResponse> ListTrustInvitationsAsync(CancellationToken ct = default);

    /// <summary>Cancels an unused invitation.</summary>
    /// <param name="invitationId">The invitation.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A task that completes when it is cancelled.</returns>
    Task CancelTrustInvitationAsync(string invitationId, CancellationToken ct = default);

    /// <summary>Removes a member from the caller's trust.</summary>
    /// <param name="userId">The member to remove.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A task that completes when they are removed.</returns>
    Task RemoveTrustMemberAsync(string userId, CancellationToken ct = default);

    /// <summary>Shows what an invite link joins, for the invite page.</summary>
    /// <param name="request">The request.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The trust name and invited email.</returns>
    Task<TrustInvitationPreviewResponse> PreviewTrustInvitationAsync(TrustInvitationTokenRequest request, CancellationToken ct = default);

    /// <summary>Joins the caller to the trust an invite link is for.</summary>
    /// <param name="request">The request.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The trust joined.</returns>
    Task<AcceptTrustInvitationResponse> AcceptTrustInvitationAsync(TrustInvitationTokenRequest request, CancellationToken ct = default);

    /// <summary>Lists the members of the caller's trust.</summary>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The members.</returns>
    Task<TrustMemberListResponse> ListTrustMembersAsync(CancellationToken ct = default);
}
