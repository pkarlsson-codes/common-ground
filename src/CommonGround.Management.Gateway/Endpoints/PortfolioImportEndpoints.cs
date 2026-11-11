using CommonGround.API.Clients;
using CommonGround.API.Contracts.PortfolioImport;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace CommonGround.Management.Gateway.Endpoints;

public static class PortfolioImportEndpoints
{
    [WolverinePost("/api/v1/portfolio-import/check")]
    [ProducesResponseType<PortfolioImportResponse>(StatusCodes.Status200OK)]
    public static async Task<IResult> CheckPortfolioImport(
        PortfolioImportRequest request,
        IPortfolioImportApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Results.Ok(await client.CheckAsync(request, cancellationToken).ConfigureAwait(false));
    }

    // A 422 from the API carries a PortfolioImportResponse with the errors; ErrorHandlingMiddleware forwards it as is.
    [WolverinePost("/api/v1/portfolio-import")]
    [ProducesResponseType<PortfolioImportResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<PortfolioImportResponse>(StatusCodes.Status422UnprocessableEntity)]
    public static async Task<IResult> ImportPortfolio(
        PortfolioImportRequest request,
        IPortfolioImportApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Results.Ok(await client.ImportAsync(request, cancellationToken).ConfigureAwait(false));
    }
}
