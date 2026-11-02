using CommonGround.API.Clients;
using CommonGround.API.Contracts.Today.Responses;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace CommonGround.Management.Gateway.Endpoints;

public static class TodayEndpoints
{
    [WolverineGet("/api/v1/today")]
    [ProducesResponseType<TodayResponse>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetToday(
        [FromQuery] string[]? areas,
        ITodayApiClient client,
        CancellationToken cancellationToken)
    {
        var result = await client.GetTodayAsync(areas, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }
}
