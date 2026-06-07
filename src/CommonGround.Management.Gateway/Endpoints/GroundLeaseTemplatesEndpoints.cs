using CommonGround.API.Clients;
using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

#pragma warning disable CA2007
#pragma warning disable SA1600

namespace CommonGround.Management.Gateway.Endpoints;

public static class GroundLeaseTemplatesEndpoints
{
    [WolverineGet("/api/v1/ground-lease-templates")]
    [ProducesResponseType<PaginatedResponse<GroundLeaseTemplateListItemResponse>>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetGroundLeaseTemplates(
        [FromQuery] GetGroundLeaseTemplatesQuerySpec querySpec,
        IGroundLeaseTemplatesApiClient client,
        CancellationToken cancellationToken = default)
    {
        var result = await client.GetGroundLeaseTemplatesAsync(querySpec, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }
}
