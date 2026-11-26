using CommonGround.API.Clients;
using CommonGround.API.Contracts.HomeBuyers.Requests;
using CommonGround.API.Contracts.HomeBuyers.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace CommonGround.Management.Gateway.Endpoints;

public static class HomeSelectionsEndpoints
{
    [WolverineGet("/api/v1/homes/{homeId}/selection")]
    [ProducesResponseType<HomeSelectionOverviewResponse>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetHomeSelectionOverview(
        string homeId,
        IHomeSelectionsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);
        var result = await client.GetHomeSelectionOverviewAsync(homeId, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverineGet("/api/v1/home-selections")]
    [ProducesResponseType<PaginatedResponse<HomeSelectionListItemResponse>>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetHomeSelections(
        [FromQuery] GetHomeSelectionsQuerySpec querySpec,
        IHomeSelectionsApiClient client,
        CancellationToken cancellationToken = default)
    {
        var result = await client.GetHomeSelectionsAsync(querySpec, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverinePost("/api/v1/homes/{homeId}/home-selections/select-by-priority")]
    [ProducesResponseType<HomeSelectionCreatedResponse>(StatusCodes.Status201Created)]
    public static async Task<IResult> SelectHouseholdByPriority(
        string homeId,
        SelectHouseholdByPriorityRequest request,
        IHomeSelectionsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);
        ArgumentNullException.ThrowIfNull(request);
        var result = await client.SelectHouseholdByPriorityAsync(homeId, request, cancellationToken).ConfigureAwait(false);
        return Results.Created($"/api/v1/homes/{homeId}/selection", result);
    }

    [WolverinePost("/api/v1/homes/{homeId}/home-selections/draw-lottery")]
    [ProducesResponseType<HomeSelectionCreatedResponse>(StatusCodes.Status201Created)]
    public static async Task<IResult> DrawSelectionLottery(
        string homeId,
        DrawSelectionLotteryRequest request,
        IHomeSelectionsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);
        ArgumentNullException.ThrowIfNull(request);
        var result = await client.DrawSelectionLotteryAsync(homeId, request, cancellationToken).ConfigureAwait(false);
        return Results.Created($"/api/v1/homes/{homeId}/selection", result);
    }
}
