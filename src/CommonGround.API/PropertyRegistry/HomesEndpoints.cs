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

        var id = HomeId.Generate();

        var command = new RegisterHome(
            id,
            LandParcelId.Create(request.LandParcelId),
            request.UnitDesignation,
            homeType,
            request.Bedrooms,
            request.Bathrooms,
            request.IsAccessible,
            request.SquareFootage,
            request.YearBuilt,
            request.InitialAssessedValue,
            request.InitialAssessedDate,
            request.AppraiserName,
            userId);

        await bus.InvokeForTenantAsync(bus.TenantId!, command, cancellationToken);

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

        var (view, availability, label) = await bus.InvokeForTenantAsync<HomeDetails>(bus.TenantId!,
            new GetHome(HomeId.Create(homeId)),
            cancellationToken);

        var response = new HomeResponse(view.Id, view.LandParcelId, label, view.UnitDesignation,
            view.UnitType, view.Bedrooms, view.Bathrooms, view.IsAccessible, view.SquareFootage,
            view.YearBuilt, view.AssessedValue, view.LastAssessedDate,
            view.ConditionNotes, view.RegisteredAt,
            availability.AvailableAgainDate, availability.SelectionOpenedDate, availability.UnderContractDate,
            availability.LeasedDate, availability.ResaleNoticeDate, availability.Occupancy, availability.SaleStage,
            availability.HeldSinceDate, availability.HoldReason, availability.AvailableAgainCause);

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
        var occupancyFilter = request.Occupancy?
            .Select(HomeOccupancy.FromDisplayName).ToList().AsReadOnly();
        var saleStageFilter = request.SaleStages?
            .Select(HomeSaleStage.FromDisplayName).ToList().AsReadOnly();
        var query = new GetHomes(request.Page, landParcelFilter, occupancyFilter, saleStageFilter);

        var result = await bus
            .InvokeForTenantAsync<PaginatedResponse<HomeListItem>>(
                bus.TenantId!, query, cancellationToken);

        var response = result.MapItems(i => new HomeListItemResponse(
            i.Home.Id, i.Home.LandParcelId, i.Label, i.Home.UnitDesignation, i.Home.UnitType, i.Home.Bedrooms,
            i.Home.Bathrooms, i.Home.SquareFootage, i.Home.AssessedValue, i.Occupancy, i.SaleStage));

        return Results.Ok(response);
    }

    [WolverinePost("/api/homes/{homeId}/release-for-sale")]
    public static async Task<IResult> ReleaseHomeForSale(
        string homeId,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);

        await bus.InvokeForTenantAsync(bus.TenantId!, new ReleaseHomeForSale(HomeId.Create(homeId)), cancellationToken);

        return Results.NoContent();
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
