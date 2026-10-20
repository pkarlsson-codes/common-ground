using CommonGround.API.Contracts.PropertyRegistry.Requests;
using CommonGround.API.Contracts.PropertyRegistry.Responses;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.PropertyRegistry.Application.Commands;
using CommonGround.PropertyRegistry.Application.Queries;
using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Domain.Homes;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace CommonGround.API.PropertyRegistry;

public static class HomesEndpoints
{
    [WolverinePost("/api/homes")]
    public static async Task<IResult> RegisterHome(
        RegisterHomeRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();

        if (string.IsNullOrWhiteSpace(request.HomeType))
        {
            return Results.BadRequest("homeType is required.");
        }

        if (!HomeType.TryFromDisplayName(request.HomeType, out var homeType))
        {
            return Results.BadRequest($"Unrecognized home type: '{request.HomeType}'.");
        }

        var command = new RegisterHome(
            LandParcelId.Create(request.LandParcelId),
            homeType,
            request.Bedrooms,
            request.Bathrooms,
            request.SquareFootage,
            request.YearBuilt,
            request.InitialAssessedValue,
            request.InitialAssessedDate,
            request.AppraiserName,
            userId);

        var id = await bus.InvokeForTenantAsync<HomeId>(bus.TenantId!, command, cancellationToken);

        return Results.Created($"/api/homes/{id.Value}", new RegisterHomeResponse(id.Value));
    }

    [WolverineGet("/api/homes/{homeId}")]
    public static async Task<IResult> GetHome(
        string homeId,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);

        var view = await bus.InvokeForTenantAsync<HomeView>(bus.TenantId!,
            new GetHome(HomeId.Create(homeId)),
            cancellationToken);

        var response = new HomeResponse(view.Id, view.LandParcelId,
            view.UnitType, view.Bedrooms, view.Bathrooms, view.SquareFootage,
            view.YearBuilt, view.AssessedValue, view.LastAssessedDate,
            view.ConditionNotes, view.Status, view.RegisteredAt);

        return Results.Ok(response);
    }

    [WolverineGet("/api/homes")]
    public static async Task<IResult> GetHomes(
        [FromQuery] GetHomesQuerySpec request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken = default)
    {
        var landParcelFilter = request.LandParcelIds?
            .Select(LandParcelId.Create).ToList().AsReadOnly();
        var statusFilter = request.Statuses?
            .Select(HomeStatus.FromDisplayName).ToList().AsReadOnly();
        var query = new GetHomes(request.Page, landParcelFilter, statusFilter);

        var result = await bus
            .InvokeForTenantAsync<PaginatedResponse<HomeListItemView>>(
                bus.TenantId!, query, cancellationToken);

        var response = result.MapItems(v => new HomeListItemResponse(
            v.Id, v.LandParcelId, v.UnitType, v.Bedrooms, v.Bathrooms, v.SquareFootage, v.AssessedValue, v.Status));

        return Results.Ok(response);
    }

    [WolverinePost("/api/homes/{homeId}/assessments")]
    public static async Task<IResult> RecordHomeAssessment(
        string homeId,
        RecordHomeAssessmentRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();

        var command = new RecordHomeAssessment(
            HomeId.Create(homeId),
            request.AssessedValue,
            request.AssessmentDate,
            request.AppraiserName,
            request.ConditionNotes,
            userId);

        await bus.InvokeForTenantAsync(bus.TenantId!, command, cancellationToken);

        return Results.NoContent();
    }

    [WolverineGet("/api/homes/{homeId}/assessments")]
    public static async Task<IResult> GetHomeAssessments(
        string homeId,
        [FromQuery] GetHomeAssessmentsQuerySpec request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);

        var query = new GetHomeAssessments(HomeId.Create(homeId), request.Page);
        var result = await bus.InvokeForTenantAsync<PaginatedResponse<HomeAssessmentView>>(bus.TenantId!, query, cancellationToken);

        var response = result.MapItems(v => new HomeAssessmentItemResponse(
            v.Id.ToString(), v.UnitId, v.AssessedValue, v.AssessmentDate, v.ConditionNotes, v.AppraiserName, v.RecordedBy, v.RecordedAt));

        return Results.Ok(response);
    }
}
