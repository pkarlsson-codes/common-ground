using CommonGround.API.Contracts.Identity.Requests;
using CommonGround.API.Contracts.Identity.Responses;
using CommonGround.Identity.Application.Commands;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CommonGround.API.Identity;

/// <summary>Manages CommonGround-owned user identity records.</summary>
[ApiController]
[Route("api/users")]
public sealed class UsersController(IMessageBus bus) : ControllerBase
{
    /// <summary>Idempotently provisions a user record for a given Keycloak subject.</summary>
    /// <param name="request">The request containing the Keycloak subject and optional profile data.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The stable owned user ID and trust membership.</returns>
    [HttpPost("ensure")]
    public async Task<IActionResult> EnsureUser(
        [FromBody] EnsureUserRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await bus.InvokeForTenantAsync<EnsureUserResult>(
            request.TrustId,
            new EnsureUser(request.KeycloakSubject, request.Email, request.DisplayName, SharedKernel.Domain.TrustId.Create(request.TrustId)),
            cancellationToken).ConfigureAwait(false);

        return Ok(new EnsureUserResponse(result.UserId, result.TrustId));
    }
}
