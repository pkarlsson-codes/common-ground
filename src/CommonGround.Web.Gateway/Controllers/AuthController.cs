using System.Security.Cryptography;
using CommonGround.Web.Gateway.Configuration;
using CommonGround.Web.Gateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace CommonGround.Web.Gateway.Controllers;

/// <summary>
/// Authentication endpoints for issuing CSRF tokens and retrieving the current user.
/// </summary>
/// <param name="env">Environment to check if production.</param>
[ApiController]
[Route("api/v1/[controller]")]
public sealed class AuthController(IHostEnvironment env) : ControllerBase
{
    /// <summary>
    /// Issues a CSRF token cookie for anonymous visitors (double-submit cookie pattern).
    /// </summary>
    /// <returns>200 OK and sets the cg_csrf cookie.</returns>
    [HttpGet("csrf")]
    [EndpointName("issueCsrfToken")]
    public IActionResult IssueCsrf()
    {
        var isProd = env.IsProduction();
        var csrfToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        Response.Cookies.Append(
            GatewayDefaults.Cookies.CsrfCookie,
            csrfToken,
            new CookieOptions
            {
                HttpOnly = false,
                SameSite = SameSiteMode.Strict,
                Secure = isProd,
                Path = "/",
                MaxAge = TimeSpan.FromDays(7),
            });
        return Ok(new { success = true });
    }

    /// <summary>
    /// Get current authenticated user.
    /// </summary>
    /// <param name="tokenStore">Token store used to resolve the current session.</param>
    /// <returns>The current user if authenticated; otherwise unauthorized.</returns>
    [HttpGet("me")]
    [EndpointName("getCurrentUser")]
    [ProducesResponseType(typeof(CommonGround.Web.Gateway.Models.CurrentUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser([FromServices] ITokenStore tokenStore)
    {
        var sessionId = Request.Cookies[GatewayDefaults.Cookies.SessionCookie];

        if (string.IsNullOrEmpty(sessionId))
        {
            return Unauthorized();
        }

        var tokens = await tokenStore.TryGetTokensAsync(sessionId, HttpContext.RequestAborted);
        if (tokens is null)
        {
            return Unauthorized();
        }

        var (userId, _, _, _, _) = tokens.Value;

        var preferredUsername = User?.FindFirst("preferred_username")?.Value
                                 ?? User?.Identity?.Name
                                 ?? string.Empty;
        var email = User?.FindFirst("email")?.Value ?? string.Empty;

        return Ok(new CommonGround.Web.Gateway.Models.CurrentUserDto
        {
            Id = userId,
            Username = string.IsNullOrWhiteSpace(preferredUsername) ? userId : preferredUsername,
            Email = email,
        });
    }
}
