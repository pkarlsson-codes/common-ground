using CommonGround.API.Clients;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

#pragma warning disable CA2007
#pragma warning disable SA1600

namespace CommonGround.Web.Gateway.Endpoints;

public static class GroundLeaseTemplatesEndpoints
{
    [WolverineGet("/api/v1/ground-lease-templates")]
    [ProducesResponseType<PaginatedResponse<GroundLeaseTemplateListItemResponse>>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetGroundLeaseTemplates(
        int pageNumber,
        int pageSize,
        string? search,
        string? status,
        IGroundLeaseTemplatesApiClient client,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber <= 0)
        {
            pageNumber = 1;
        }

        if (pageSize <= 0)
        {
            pageSize = 20;
        }

        var result = await client.GetGroundLeaseTemplatesAsync(pageNumber, pageSize, search, status, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }
}
