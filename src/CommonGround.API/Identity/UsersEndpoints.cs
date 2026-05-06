using CommonGround.API.Contracts.Identity.Requests;
using CommonGround.API.Contracts.Identity.Responses;
using CommonGround.Identity.Application.Commands;
using CommonGround.SharedKernel.Domain;
using Wolverine;
using Wolverine.Http;

namespace CommonGround.API.Identity;

/// <summary>
/// Wolverine HTTP endpoints for user identity operations.
/// </summary>
public static class UsersEndpoints
{
    /// <summary>
    /// Idempotently provisions a user record for a Keycloak subject.
    /// </summary>
    /// <param name="request">The request containing the Keycloak subject and optional profile data.</param>
    /// <param name="bus">The Wolverine message bus.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The stable owned user ID and trust membership.</returns>
    [WolverinePost("/api/users/ensure")]
    [NotTenanted]
    public static async Task<IResult> EnsureUser(
        EnsureUserRequest request,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await bus.InvokeForTenantAsync<EnsureUserResult>(
            request.TrustId,
            new EnsureUser(request.KeycloakSubject, request.Email, request.DisplayName, TrustId.Create(request.TrustId)),
            cancellationToken).ConfigureAwait(false);

        return Results.Ok(new EnsureUserResponse(result.UserId, result.TrustId));
    }
}