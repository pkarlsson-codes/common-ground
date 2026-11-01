using CommonGround.API.Contracts.PropertyRegistry.Requests;
using CommonGround.API.Contracts.PropertyRegistry.Responses;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.PropertyRegistry.Application.Commands;
using CommonGround.PropertyRegistry.Application.Queries;
using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace CommonGround.API.PropertyRegistry;

public static class LandParcelsEndpoints
{
    [WolverinePost("/api/land-parcels")]
    public static async Task<IResult> RegisterLandParcel(
        RegisterLandParcelRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();
        var acquisitionMethod = AcquisitionMethod.FromDisplayName(request.AcquisitionMethod);

        var id = LandParcelId.Generate();

        var command = new RegisterLandParcel(
            id,
            request.LegalParcelNumber,
            request.Address,
            request.AreaAcres,
            request.AcquisitionDate,
            acquisitionMethod,
            request.CovenantTerms,
            userId);

        await bus.InvokeForTenantAsync(bus.TenantId!, command, cancellationToken);

        return Results.Created($"/api/land-parcels/{id.Value}", new RegisterLandParcelResponse(id.Value));
    }

    [WolverineGet("/api/land-parcels/{landParcelId}")]
    public static async Task<IResult> GetLandParcel(
        string landParcelId,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(landParcelId);

        var view = await bus.InvokeForTenantAsync<LandParcelView>(bus.TenantId!,
            new GetLandParcel(LandParcelId.Create(landParcelId)),
            cancellationToken);

        var response = new LandParcelResponse(
            view.Id, view.LegalParcelNumber, view.Address, view.AreaAcres,
            view.AcquisitionDate, view.AcquisitionMethod, view.CovenantTerms,
            view.IsDecommissioned, view.RegisteredAt, view.RegisteredBy);

        return Results.Ok(response);
    }

    [WolverineGet("/api/land-parcels")]
    public static async Task<IResult> GetLandParcels(
        [FromQuery] GetLandParcelsQuerySpec request,
        IMessageBus bus,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLandParcels(request.Page, request.Search, request.IncludeDecommissioned);

        var result = await bus
            .InvokeForTenantAsync<PaginatedResponse<LandParcelListItemView>>(
                bus.TenantId!, query, cancellationToken);

        var response = result.MapItems(v => new LandParcelListItemResponse(
            v.Id, v.LegalParcelNumber, v.Address, v.AreaAcres, v.IsDecommissioned, v.RegisteredAt));

        return Results.Ok(response);
    }

    [WolverinePost("/api/land-parcels/{landParcelId}/decommission")]
    public static async Task<IResult> DecommissionLandParcel(
        string landParcelId,
        DecommissionParcelRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(landParcelId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new DecommissionParcel(LandParcelId.Create(landParcelId), userId),
            cancellationToken).ConfigureAwait(false);

        return Results.NoContent();
    }

    [WolverinePost("/api/land-parcels/{landParcelId}/recommission")]
    public static async Task<IResult> RecommissionLandParcel(
        string landParcelId,
        RecommissionParcelRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(landParcelId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new RecommissionParcel(LandParcelId.Create(landParcelId), userId),
            cancellationToken).ConfigureAwait(false);

        return Results.NoContent();
    }
}
