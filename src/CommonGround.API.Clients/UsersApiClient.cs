using CommonGround.API.Contracts.Identity.Requests;
using CommonGround.API.Contracts.Identity.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.Extensions.Logging;

namespace CommonGround.API.Clients;

/// <summary>
/// HTTP client for identity user API endpoints.
/// Requests are decorated with a service token by <c>InternalServiceTokenHandler</c>,
/// because during the OIDC callback <c>HttpContext.User</c> is not yet populated.
/// </summary>
public sealed class UsersApiClient(
    IHttpClientFactory httpClientFactory,
    ILogger<UsersApiClient> logger) : BaseApiClient(httpClientFactory.CreateClient(nameof(UsersApiClient)), logger), IUsersApiClient
{
    /// <inheritdoc/>
    public async Task<EnsureUserResponse> EnsureUserAsync(EnsureUserRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await PostAsync<EnsureUserRequest, EnsureUserResponse>("api/users/ensure", request, ct).ConfigureAwait(false);
    }
}
