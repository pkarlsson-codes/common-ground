using CommonGround.API.Clients;
using CommonGround.API.Contracts.PropertyRegistry.Requests;
using CommonGround.API.Contracts.PropertyRegistry.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace CommonGround.Management.Gateway.Endpoints;

public static class LandParcelsEndpoints
{
    [WolverinePost("/api/v1/land-parcels")]
    [ProducesResponseType<RegisterLandParcelResponse>(StatusCodes.Status201Created)]
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
    [ProducesResponseType<LandParcelResponse>(StatusCodes.Status200OK)]
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
    [ProducesResponseType<PaginatedResponse<LandParcelListItemResponse>>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetLandParcels(
        [FromQuery] GetLandParcelsQuerySpec querySpec,
        ILandParcelsApiClient client,
        CancellationToken cancellationToken = default)
    {
        var result = await client.GetLandParcelsAsync(querySpec, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverinePost("/api/v1/land-parcels/{landParcelId}/decommission")]
    [EndpointName("DecommissionLandParcel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
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
    [EndpointName("RecommissionLandParcel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
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
