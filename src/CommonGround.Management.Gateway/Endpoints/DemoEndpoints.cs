using CommonGround.API.Clients;
using CommonGround.API.Contracts.Demo.Responses;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace CommonGround.Management.Gateway.Endpoints;

public static class DemoEndpoints
{
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
}
