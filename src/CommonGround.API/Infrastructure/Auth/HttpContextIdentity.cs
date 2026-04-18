using CommonGround.SharedKernel.Domain;

namespace CommonGround.API.Infrastructure.Auth;

/// <summary>
/// Provides extension methods for retrieving user identity information from the HTTP context.
/// </summary>
internal static class HttpContextIdentity
{
    private const string UserIdClaimType = "user_id";

    /// <summary>
    /// Retrieves the user ID from the HTTP context.
    /// </summary>
    /// <param name="httpContext">The HTTP context containing the user claims.</param>
    /// <returns>The user ID extracted from the HTTP context.</returns>
    public static UserId GetUserId(this HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var id = httpContext.User?.FindFirst(UserIdClaimType)?.Value
            ?? throw new InvalidOperationException($"Claim '{UserIdClaimType}' not found. Ensure the endpoint is authenticated.");

        return UserId.Create(id);
    }
}
