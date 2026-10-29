using CommonGround.API.Clients;
using CommonGround.API.Contracts.TrustManagement.Requests;
using CommonGround.API.Contracts.TrustManagement.Responses;
using CommonGround.Management.Gateway.Configuration;
using CommonGround.Management.Gateway.Services;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace CommonGround.Management.Gateway.Endpoints;

public static class TrustsEndpoints
{
    [WolverineGet("/api/v1/trusts/current")]
    [ProducesResponseType<TrustResponse>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetCurrentTrust(
        ITrustsApiClient client,
        CancellationToken cancellationToken)
    {
        var result = await client.GetCurrentTrustAsync(cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverinePost("/api/v1/trusts")]
    [ProducesResponseType<CreateTrustResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public static async Task<IResult> CreateTrust(
        CreateTrustRequest request,
        ITrustsApiClient client,
        ITokenStore tokenStore,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var result = await client.CreateTrustAsync(request, cancellationToken).ConfigureAwait(false);

        var sessionId = httpContext.Request.Cookies[GatewayDefaults.Cookies.SessionCookie];
        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            var tokens = await tokenStore.TryGetTokensAsync(sessionId, cancellationToken).ConfigureAwait(false);
            if (tokens is not null)
            {
                var (userId, _, accessToken, refreshToken, idToken) = tokens.Value;
                await tokenStore.SaveTokensAsync(sessionId, userId, result.TrustId, accessToken, refreshToken, idToken, cancellationToken).ConfigureAwait(false);
            }
        }

        return Results.Ok(result);
    }
}
