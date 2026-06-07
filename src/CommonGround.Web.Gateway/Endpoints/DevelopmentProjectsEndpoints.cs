using CommonGround.API.Clients;
using CommonGround.API.Contracts.PropertyRegistry.Requests;
using CommonGround.API.Contracts.PropertyRegistry.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

#pragma warning disable CA2007
#pragma warning disable SA1600

namespace CommonGround.Web.Gateway.Endpoints;

public static class DevelopmentProjectsEndpoints
{
    [WolverineGet("/api/v1/development-projects")]
    [ProducesResponseType<PaginatedResponse<DevelopmentProjectListItemResponse>>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetDevelopmentProjects(
        [FromQuery] GetDevelopmentProjectsQuerySpec querySpec,
        IDevelopmentProjectsApiClient client,
        CancellationToken cancellationToken = default)
    {
        var result = await client.GetDevelopmentProjectsAsync(querySpec, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverinePost("/api/v1/development-projects")]
    [ProducesResponseType<InitiateDevelopmentProjectResponse>(StatusCodes.Status201Created)]
    public static async Task<IResult> InitiateDevelopmentProject(
        InitiateDevelopmentProjectRequest request,
        IDevelopmentProjectsApiClient client,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var result = await client.InitiateDevelopmentProjectAsync(request, cancellationToken).ConfigureAwait(false);
        return Results.Created($"/api/v1/development-projects/{result.ProjectId}", result);
    }
}
