using CommonGround.API.Clients;
using CommonGround.API.Contracts.HomeBuyers.Requests;
using CommonGround.API.Contracts.HomeBuyers.Responses;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace CommonGround.Management.Gateway.Endpoints;

public static class HomeSelectionsEndpoints
{
    [WolverinePost("/api/v1/homes/{homeId}/home-selections")]
    [ProducesResponseType<StartHomeSelectionResponse>(StatusCodes.Status201Created)]
    public static async Task<IResult> StartHomeSelection(
        string homeId,
        IHomeSelectionsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);
        var result = await client.StartHomeSelectionAsync(homeId, cancellationToken).ConfigureAwait(false);
        return Results.Created($"/api/v1/home-selections/{result.SelectionId}", result);
    }

    [WolverineGet("/api/v1/homes/{homeId}/home-selections")]
    [ProducesResponseType<IReadOnlyList<HomeSelectionSummaryResponse>>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetHomeSelectionsForHome(
        string homeId,
        IHomeSelectionsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);
        var result = await client.GetHomeSelectionsForHomeAsync(homeId, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverineGet("/api/v1/home-selections/{selectionId}")]
    [ProducesResponseType<HomeSelectionResponse>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetHomeSelection(
        string selectionId,
        IHomeSelectionsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(selectionId);
        var result = await client.GetHomeSelectionAsync(selectionId, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverinePost("/api/v1/home-selections/{selectionId}/select-by-priority")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> SelectHouseholdByPriority(
        string selectionId,
        SelectHouseholdByPriorityRequest request,
        IHomeSelectionsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(selectionId);
        ArgumentNullException.ThrowIfNull(request);
        await client.SelectHouseholdByPriorityAsync(selectionId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/home-selections/{selectionId}/draw-lottery")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> DrawSelectionLottery(
        string selectionId,
        DrawSelectionLotteryRequest request,
        IHomeSelectionsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(selectionId);
        ArgumentNullException.ThrowIfNull(request);
        await client.DrawSelectionLotteryAsync(selectionId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/home-selections/{selectionId}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> CancelHomeSelection(
        string selectionId,
        CancelHomeSelectionRequest request,
        IHomeSelectionsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(selectionId);
        ArgumentNullException.ThrowIfNull(request);
        await client.CancelHomeSelectionAsync(selectionId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }
}
