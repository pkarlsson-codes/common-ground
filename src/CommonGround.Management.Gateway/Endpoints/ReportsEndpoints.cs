using CommonGround.API.Clients;
using CommonGround.API.Contracts.Reports.Responses;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace CommonGround.Management.Gateway.Endpoints;

public static class ReportsEndpoints
{
    [WolverineGet("/api/v1/reports/funder")]
    [ProducesResponseType<FunderReportResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public static async Task<IResult> GetFunderReport(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        IReportsApiClient client,
        CancellationToken cancellationToken)
    {
        var result = await client.GetFunderReportAsync(from, to, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }
}
