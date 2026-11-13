using CommonGround.API.Clients;
using CommonGround.API.Contracts.Demo.Responses;
using CommonGround.Management.Gateway.Configuration;
using CommonGround.Management.Gateway.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace CommonGround.Management.Gateway.Endpoints;

public static class DemoEndpoints
{
    // Read by the signed-out Welcome page, so it needs no session. 404 when the server is not in demo mode.
    [WolverineGet("/api/v1/demo/mode")]
    [AllowAnonymous]
    [ProducesResponseType<DemoModeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> GetMode(
        IDemoApiClient client,
        CancellationToken cancellationToken)
    {
        var result = await client.GetModeAsync(cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    // 404 when the server is not in demo mode: the app shows no demo banner.
    [WolverineGet("/api/v1/demo")]
    [ProducesResponseType<DemoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> GetDemo(
        IDemoApiClient client,
        CancellationToken cancellationToken)
    {
        var result = await client.GetDemoAsync(cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverinePost("/api/v1/demo/sandboxes")]
    [ProducesResponseType<DemoSandboxResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public static async Task<IResult> StartSandbox(
        IDemoApiClient client,
        CancellationToken cancellationToken)
    {
        var result = await client.StartSandboxAsync(cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    // When the sample trust is ready, puts it on the session so the next request is made inside it.
    [WolverineGet("/api/v1/demo/sandboxes/current")]
    [ProducesResponseType<CurrentDemoSandboxResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> GetCurrentSandbox(
        IDemoApiClient client,
        ITokenStore tokenStore,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await client.GetCurrentSandboxAsync(cancellationToken).ConfigureAwait(false);

        var sessionId = httpContext.Request.Cookies[GatewayDefaults.Cookies.SessionCookie];
        if (result.Sandbox is { Status: "Ready" } sandbox && !string.IsNullOrWhiteSpace(sessionId))
        {
            var tokens = await tokenStore.TryGetTokensAsync(sessionId, cancellationToken).ConfigureAwait(false);
            if (tokens is { trustId: null } stored)
            {
                await tokenStore.SaveTokensAsync(sessionId, stored.userId, sandbox.TrustId, stored.accessToken, stored.refreshToken, stored.idToken, cancellationToken).ConfigureAwait(false);
            }
        }

        return Results.Ok(result);
    }
}
