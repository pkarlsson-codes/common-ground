using CommonGround.API.Clients;
using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;


namespace CommonGround.Management.Gateway.Endpoints;

public static class GroundLeaseTemplatesEndpoints
{
    [WolverinePost("/api/v1/ground-lease-templates")]
    [ProducesResponseType<CreateGroundLeaseTemplateResponse>(StatusCodes.Status201Created)]
    public static async Task<IResult> CreateGroundLeaseTemplate(
        CreateGroundLeaseTemplateRequest request,
        IGroundLeaseTemplatesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var result = await client.CreateGroundLeaseTemplateAsync(request, cancellationToken).ConfigureAwait(false);
        return Results.Created($"/api/v1/ground-lease-templates/{result.GroundLeaseTemplateId}", result);
    }

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
