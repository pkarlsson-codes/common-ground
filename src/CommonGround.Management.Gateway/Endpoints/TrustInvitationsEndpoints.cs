using CommonGround.API.Clients;
using CommonGround.API.Contracts.TrustManagement.Requests;
using CommonGround.API.Contracts.TrustManagement.Responses;
using CommonGround.Management.Gateway.Configuration;
using CommonGround.Management.Gateway.Services;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace CommonGround.Management.Gateway.Endpoints;

public static class TrustInvitationsEndpoints
{
    [WolverinePost("/api/v1/trust-invitations")]
    [ProducesResponseType<CreateTrustInvitationResponse>(StatusCodes.Status200OK)]
    public static async Task<IResult> CreateTrustInvitation(
        CreateTrustInvitationRequest request,
        ITrustsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Results.Ok(await client.CreateTrustInvitationAsync(request, cancellationToken).ConfigureAwait(false));
    }

    [WolverineGet("/api/v1/trust-invitations")]
    [ProducesResponseType<TrustInvitationListResponse>(StatusCodes.Status200OK)]
    public static async Task<IResult> ListTrustInvitations(
        ITrustsApiClient client,
        CancellationToken cancellationToken)
        => Results.Ok(await client.ListTrustInvitationsAsync(cancellationToken).ConfigureAwait(false));

    [WolverinePost("/api/v1/trust-invitations/{invitationId}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> CancelTrustInvitation(
        string invitationId,
        ITrustsApiClient client,
        CancellationToken cancellationToken)
    {
        await client.CancelTrustInvitationAsync(invitationId, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/trust-invitations/preview")]
    [ProducesResponseType<TrustInvitationPreviewResponse>(StatusCodes.Status200OK)]
    public static async Task<IResult> PreviewTrustInvitation(
        TrustInvitationTokenRequest request,
        ITrustsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Results.Ok(await client.PreviewTrustInvitationAsync(request, cancellationToken).ConfigureAwait(false));
    }

    // Joins the trust, then puts the trust on the session so the next request is made inside it.
    [WolverinePost("/api/v1/trust-invitations/accept")]
    [ProducesResponseType<AcceptTrustInvitationResponse>(StatusCodes.Status200OK)]
    public static async Task<IResult> AcceptTrustInvitation(
        TrustInvitationTokenRequest request,
        ITrustsApiClient client,
        ITokenStore tokenStore,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var result = await client.AcceptTrustInvitationAsync(request, cancellationToken).ConfigureAwait(false);

        var sessionId = httpContext.Request.Cookies[GatewayDefaults.Cookies.SessionCookie];
        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            var tokens = await tokenStore.TryGetTokensAsync(sessionId, cancellationToken).ConfigureAwait(false);
            if (tokens is not null)
            {
                var (userId, _, accessToken, refreshToken, idToken) = tokens.Value;
                await tokenStore.SaveTokensAsync(sessionId, userId, result.TrustId, accessToken, refreshToken, idToken, cancellationToken).ConfigureAwait(false);
            }
        }

        return Results.Ok(result);
    }

    [WolverineGet("/api/v1/trust-members")]
    [ProducesResponseType<TrustMemberListResponse>(StatusCodes.Status200OK)]
    public static async Task<IResult> ListTrustMembers(
        ITrustsApiClient client,
        CancellationToken cancellationToken)
        => Results.Ok(await client.ListTrustMembersAsync(cancellationToken).ConfigureAwait(false));

    [WolverinePost("/api/v1/trust-members/{userId}/remove")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> RemoveTrustMember(
        string userId,
        ITrustsApiClient client,
        CancellationToken cancellationToken)
    {
        await client.RemoveTrustMemberAsync(userId, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }
}
