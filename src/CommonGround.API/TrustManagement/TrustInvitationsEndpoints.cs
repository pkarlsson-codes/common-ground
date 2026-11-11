using CommonGround.API.Contracts.TrustManagement.Requests;
using CommonGround.API.Contracts.TrustManagement.Responses;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.Identity.Application.Queries;
using CommonGround.Identity.Application.ReadModels;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.TrustManagement.Application.Commands;
using CommonGround.TrustManagement.Application.Errors;
using CommonGround.TrustManagement.Application.Queries;
using CommonGround.TrustManagement.Application.ReadModels;
using CommonGround.TrustManagement.Application.Services;
using CommonGround.TrustManagement.Domain.TrustInvitations;
using CommonGround.TrustManagement.Domain.Trusts;
using Microsoft.AspNetCore.Authorization;
using Wolverine;
using Wolverine.Http;

namespace CommonGround.API.TrustManagement;

// Invitations and users are stored in the default tenant (a user without a trust has to find an invitation
// by its link), so every endpoint here is [NotTenanted] and checks the caller's trust from the claim itself.
public static class TrustInvitationsEndpoints
{
    [WolverinePost("/api/trust-invitations")]
    [NotTenanted]
    [Authorize]
    public static async Task<IResult> CreateTrustInvitation(
        CreateTrustInvitationRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (httpContext.TryGetTenantId() is not { } trustId)
        {
            return Results.Forbid();
        }

        var invitationId = TrustInvitationId.Generate();
        var token = InvitationToken.Generate();
        var now = TimeProviderContext.Current.UtcNow;

        await bus.InvokeAsync(
            new IssueTrustInvitation(invitationId, trustId, request.Email, InvitationToken.Hash(token), httpContext.GetUserId(), now),
            cancellationToken).ConfigureAwait(false);

        return Results.Ok(new CreateTrustInvitationResponse(invitationId.Value, token, now.Add(TrustInvitation.Lifetime)));
    }

    [WolverineGet("/api/trust-invitations")]
    [NotTenanted]
    [Authorize]
    public static async Task<IResult> ListTrustInvitations(
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        if (httpContext.TryGetTenantId() is not { } trustId)
        {
            return Results.Forbid();
        }

        var invitations = await bus.InvokeAsync<IReadOnlyList<TrustInvitationView>>(
            new ListPendingTrustInvitations(trustId, TimeProviderContext.Current.UtcNow), cancellationToken).ConfigureAwait(false);
        var members = await bus.InvokeAsync<IReadOnlyList<UserView>>(new ListTrustMembers(trustId), cancellationToken).ConfigureAwait(false);
        var names = members.ToDictionary(m => m.UserId, m => m.DisplayName ?? m.Email);

        return Results.Ok(new TrustInvitationListResponse(invitations
            .Select(i => new TrustInvitationResponse(i.Id, i.Email, names.GetValueOrDefault(i.IssuedBy), i.IssuedAt, i.ExpiresAt))
            .ToList()));
    }

    [WolverinePost("/api/trust-invitations/{invitationId}/cancel")]
    [NotTenanted]
    [Authorize]
    public static async Task<IResult> CancelTrustInvitation(
        string invitationId,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        if (httpContext.TryGetTenantId() is not { } trustId)
        {
            return Results.Forbid();
        }

        await bus.InvokeAsync(
            new CancelTrustInvitation(TrustInvitationId.Create(invitationId), trustId, httpContext.GetUserId()),
            cancellationToken).ConfigureAwait(false);

        return Results.NoContent();
    }

    [WolverinePost("/api/trust-invitations/preview")]
    [NotTenanted]
    [Authorize]
    public static async Task<IResult> PreviewTrustInvitation(
        TrustInvitationTokenRequest request,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var invitation = await bus.InvokeAsync<TrustInvitationView?>(
            new FindTrustInvitation(InvitationToken.Hash(request.Token)), cancellationToken).ConfigureAwait(false);
        if (invitation is null
            || invitation.Status != TrustInvitationView.Pending
            || invitation.ExpiresAt <= TimeProviderContext.Current.UtcNow)
        {
            throw BusinessRuleViolation.TrustInvitation.Accept.NoLongerValid();
        }

        var trust = await bus.InvokeAsync<Trust?>(new GetTrust(invitation.TrustId), cancellationToken).ConfigureAwait(false)
            ?? throw BusinessRuleViolation.TrustInvitation.Accept.NoLongerValid();

        return Results.Ok(new TrustInvitationPreviewResponse(trust.Name, invitation.Email));
    }

    [WolverinePost("/api/trust-invitations/accept")]
    [NotTenanted]
    [Authorize]
    public static async Task<IResult> AcceptTrustInvitation(
        TrustInvitationTokenRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (httpContext.TryGetTenantId() is not null)
        {
            throw BusinessRuleViolation.TrustInvitation.Accept.AlreadyInTrust();
        }

        var trustId = await bus.InvokeAsync<TrustId>(
            new AcceptTrustInvitation(InvitationToken.Hash(request.Token), httpContext.GetUserId(), TimeProviderContext.Current.UtcNow),
            cancellationToken).ConfigureAwait(false);

        return Results.Ok(new AcceptTrustInvitationResponse(trustId.Value));
    }

    [WolverineGet("/api/trust-members")]
    [NotTenanted]
    [Authorize]
    public static async Task<IResult> ListTrustMembers(
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        if (httpContext.TryGetTenantId() is not { } trustId)
        {
            return Results.Forbid();
        }

        var members = await bus.InvokeAsync<IReadOnlyList<UserView>>(new ListTrustMembers(trustId), cancellationToken).ConfigureAwait(false);

        return Results.Ok(new TrustMemberListResponse(members
            .Select(m => new TrustMemberResponse(m.UserId, m.DisplayName, m.Email))
            .ToList()));
    }
}
