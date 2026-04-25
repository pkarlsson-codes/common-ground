using CommonGround.API.Contracts.Identity.Requests;
using CommonGround.API.Contracts.Identity.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.Extensions.Logging;

namespace CommonGround.Web.Gateway.Clients;

/// <summary>
/// HTTP client for identity user API endpoints.
/// Requests are decorated with a service token by <c>InternalServiceTokenHandler</c>,
/// because during the OIDC callback <c>HttpContext.User</c> is not yet populated.
/// </summary>
public sealed class UsersApiClient(
    HttpClient httpClient,
    ILogger<UsersApiClient> logger) : BaseApiClient(httpClient, logger), IUsersApiClient
{
    /// <inheritdoc/>
    public async Task<EnsureUserResponse> EnsureUserAsync(EnsureUserRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await PostAsync<EnsureUserRequest, EnsureUserResponse>("api/users/ensure", request, ct).ConfigureAwait(false);
    }
}
