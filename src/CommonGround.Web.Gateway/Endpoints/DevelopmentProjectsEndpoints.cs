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
        int pageNumber,
        int pageSize,
        string? search,
        string? landParcelId,
        IDevelopmentProjectsApiClient client,
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

        var result = await client.GetDevelopmentProjectsAsync(pageNumber, pageSize, search, landParcelId, null, cancellationToken).ConfigureAwait(false);
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
