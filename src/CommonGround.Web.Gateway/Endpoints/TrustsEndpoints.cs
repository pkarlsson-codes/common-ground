using CommonGround.API.Clients;
using CommonGround.API.Contracts.TrustManagement.Requests;
using CommonGround.API.Contracts.TrustManagement.Responses;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

#pragma warning disable CA2007
#pragma warning disable SA1600

namespace CommonGround.Web.Gateway.Endpoints;

public static class TrustsEndpoints
{
    [WolverinePost("/api/v1/trusts")]
    [ProducesResponseType<CreateTrustResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public static async Task<IResult> CreateTrust(
        CreateTrustRequest request,
        ITrustsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var result = await client.CreateTrustAsync(request, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }
}
