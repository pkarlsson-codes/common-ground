using System.Security.Claims;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.API.Infrastructure.Auth;

/// <summary>
/// Provides claim extraction helpers for Wolverine HTTP endpoints.
/// </summary>
internal static class WolverineEndpointAuth
{
    private const string UserIdClaimType = "user_id";
    private const string TenantIdClaimType = "tenant_id";

    /// <summary>
    /// Attempts to extract the required identity claims for endpoint execution.
    /// Returns an unauthorized problem result when extraction fails.
    /// </summary>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <param name="userId">The extracted user identifier.</param>
    /// <param name="tenantId">The extracted tenant identifier.</param>
    /// <param name="failure">A response result describing why extraction failed.</param>
    /// <returns>True when all required claims are available and valid; otherwise false.</returns>
    public static bool TryGetRequiredIdentity(
        this HttpContext httpContext,
        out UserId? userId,
        out string? tenantId,
        out IResult? failure)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        userId = null;
        tenantId = null;

        if (!TryGetRequiredClaim(httpContext.User, UserIdClaimType, out var userIdValue, out failure))
        {
            return false;
        }

        if (!TryGetRequiredClaim(httpContext.User, TenantIdClaimType, out tenantId, out failure))
        {
            return false;
        }

        try
        {
            userId = UserId.Create(userIdValue!);
            failure = null;
            return true;
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException or InvalidOperationException)
        {
            failure = WolverineEndpointResponses.UnauthorizedProblem("Claim 'user_id' is invalid.");
            return false;
        }
    }

    private static bool TryGetRequiredClaim(
        ClaimsPrincipal? principal,
        string claimType,
        out string? claimValue,
        out IResult? failure)
    {
        claimValue = null;

        if (principal?.Identity?.IsAuthenticated != true)
        {
            failure = WolverineEndpointResponses.UnauthorizedProblem("Authentication is required.");
            return false;
        }

        claimValue = principal.FindFirst(claimType)?.Value;
        if (string.IsNullOrWhiteSpace(claimValue))
        {
            failure = WolverineEndpointResponses.UnauthorizedProblem($"Claim '{claimType}' not found. Ensure the endpoint is authenticated.");
            return false;
        }

        failure = null;
        return true;
    }
}

/// <summary>
/// Shared response helpers for Wolverine HTTP endpoint migration.
/// </summary>
internal static class WolverineEndpointResponses
{
    /// <summary>
    /// Creates a standardized 401 ProblemDetails response.
    /// </summary>
    /// <param name="detail">A detail message for the unauthorized response.</param>
    /// <returns>An <see cref="IResult"/> containing a 401 problem payload.</returns>
    public static IResult UnauthorizedProblem(string detail)
    {
        return Results.Problem(
            title: "Unauthorized",
            detail: detail,
            statusCode: StatusCodes.Status401Unauthorized);
    }
}