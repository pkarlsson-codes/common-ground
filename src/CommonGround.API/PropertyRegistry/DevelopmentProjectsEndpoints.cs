using CommonGround.API.Contracts.PropertyRegistry.Requests;
using CommonGround.API.Contracts.PropertyRegistry.Responses;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.PropertyRegistry.Application.Commands;
using CommonGround.PropertyRegistry.Application.Queries;
using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Domain.DevelopmentProjects;
using CommonGround.PropertyRegistry.Domain.Homes;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace CommonGround.API.PropertyRegistry;

public static class DevelopmentProjectsEndpoints
{
    [WolverineGet("/api/development-projects")]
    public static async Task<IResult> GetDevelopmentProjects(
        [FromQuery] GetDevelopmentProjectsQuerySpec request,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        var query = new GetDevelopmentProjects(
            request.Page,
            request.Search,
            request.LandParcelId,
            request.HomeIds);

        var result = await bus.InvokeForTenantAsync<PaginatedResponse<DevelopmentProjectListItemView>>(bus.TenantId!, query, cancellationToken);

        var response = result.MapItems(v => new DevelopmentProjectListItemResponse(
            v.Id, v.LandParcelId, v.Name, v.Description, v.Status, v.ProjectType));

        return Results.Ok(response);
    }

    [WolverineGet("/api/development-projects/{projectId}")]
    public static async Task<IResult> GetDevelopmentProject(
        string projectId,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectId);

        var view = await bus.InvokeForTenantAsync<DevelopmentProjectListItemView>(bus.TenantId!,
            new GetDevelopmentProject(DevelopmentProjectId.Create(projectId)),
            cancellationToken);

        return Results.Ok(new DevelopmentProjectResponse(
            view.Id, view.LandParcelId, view.Name, view.Description, view.Status, view.ProjectType, view.HomeIds));
    }

    [WolverinePost("/api/development-projects")]
    public static async Task<IResult> InitiateDevelopmentProject(
        InitiateDevelopmentProjectRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();
        var landParcelId = LandParcelId.Create(request.LandParcelId);

        if (!ProjectType.TryFromDisplayName(request.Type, out var projectType))
        {
            return Results.BadRequest($"Unrecognized project type: '{request.Type}'.");
        }

        DevelopmentProjectId id = projectType == ProjectType.NewConstruction
            ? await bus.InvokeForTenantAsync<DevelopmentProjectId>(bus.TenantId!,
                new InitiateNewConstructionProject(landParcelId, request.Name, request.Description, userId),
                cancellationToken)
            : await bus.InvokeForTenantAsync<DevelopmentProjectId>(bus.TenantId!,
                new InitiateRehabilitationProject(
                    landParcelId,
                    (request.ExistingHomes ?? []).Select(h => HomeId.Create(h)).ToList(),
                    request.Name,
                    request.Description,
                    userId),
                cancellationToken);

        return Results.Created("/api/development-projects", new InitiateDevelopmentProjectResponse(id.Value));
    }
}
