using CommonGround.API.Clients;
using CommonGround.API.Contracts.PropertyRegistry.Requests;
using CommonGround.API.Contracts.PropertyRegistry.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Wolverine.Http;

#pragma warning disable CA2007
#pragma warning disable SA1600

namespace CommonGround.Web.Gateway.Endpoints;

public static class LandParcelsEndpoints
{
    [WolverinePost("/api/v1/land-parcels")]
    public static async Task<IResult> RegisterLandParcel(
        RegisterLandParcelRequest request,
        ILandParcelsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var result = await client.RegisterLandParcelAsync(request, cancellationToken).ConfigureAwait(false);
        return Results.Created($"/api/v1/land-parcels/{result.ParcelId}", result);
    }

    [WolverineGet("/api/v1/land-parcels/{landParcelId}")]
    public static async Task<IResult> GetLandParcel(
        string landParcelId,
        ILandParcelsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(landParcelId);
        var result = await client.GetLandParcelAsync(landParcelId, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverineGet("/api/v1/land-parcels")]
    public static async Task<IResult> GetLandParcels(
        int pageNumber,
        int pageSize,
        string? search,
        ILandParcelsApiClient client,
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

        var result = await client.GetLandParcelsAsync(pageNumber, pageSize, search, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverinePost("/api/v1/land-parcels/{landParcelId}/decommission")]
    public static async Task<IResult> DecommissionParcel(
        string landParcelId,
        DecommissionParcelRequest request,
        ILandParcelsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(landParcelId);
        ArgumentNullException.ThrowIfNull(request);
        await client.DecommissionParcelAsync(landParcelId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/land-parcels/{landParcelId}/recommission")]
    public static async Task<IResult> RecommissionParcel(
        string landParcelId,
        RecommissionParcelRequest request,
        ILandParcelsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(landParcelId);
        ArgumentNullException.ThrowIfNull(request);
        await client.RecommissionParcelAsync(landParcelId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }
}
