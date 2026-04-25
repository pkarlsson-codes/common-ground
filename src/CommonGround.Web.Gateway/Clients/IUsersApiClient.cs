using CommonGround.API.Contracts.Identity.Requests;
using CommonGround.API.Contracts.Identity.Responses;

namespace CommonGround.Web.Gateway.Clients;

/// <summary>HTTP client for the identity/users API endpoints.</summary>
public interface IUsersApiClient
{
    /// <summary>Idempotently provisions a CommonGround-owned user record and returns the response.</summary>
    /// <param name="request">The request containing the Keycloak subject and optional profile data.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The response containing the stable owned user ID.</returns>
    Task<EnsureUserResponse> EnsureUserAsync(EnsureUserRequest request, CancellationToken ct = default);
}
