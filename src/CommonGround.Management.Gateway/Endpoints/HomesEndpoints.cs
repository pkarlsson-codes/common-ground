using CommonGround.API.Clients;
using CommonGround.API.Contracts.PropertyRegistry.Requests;
using CommonGround.API.Contracts.PropertyRegistry.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;


namespace CommonGround.Management.Gateway.Endpoints;

public static class HomesEndpoints
{
    [WolverinePost("/api/v1/homes")]
    [ProducesResponseType<RegisterHomeResponse>(StatusCodes.Status201Created)]
    public static async Task<IResult> RegisterHome(
        RegisterHomeRequest request,
        IHomesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var result = await client.RegisterHomeAsync(request, cancellationToken).ConfigureAwait(false);
        return Results.Created($"/api/v1/homes/{result.HomeId}", result);
    }

    [WolverineGet("/api/v1/homes/{homeId}")]
    [ProducesResponseType<HomeResponse>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetHome(
        string homeId,
        IHomesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);
        var result = await client.GetHomeAsync(homeId, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverineGet("/api/v1/homes")]
    [ProducesResponseType<PaginatedResponse<HomeListItemResponse>>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetHomes(
        [FromQuery] GetHomesQuerySpec querySpec,
        IHomesApiClient client,
        CancellationToken cancellationToken = default)
    {
        var result = await client.GetHomesAsync(querySpec, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverinePost("/api/v1/homes/{homeId}/assessments")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public static async Task<IResult> RecordHomeAssessment(
        string homeId,
        RecordHomeAssessmentRequest request,
        IHomesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);
        ArgumentNullException.ThrowIfNull(request);
        await client.RecordHomeAssessmentAsync(homeId, request, cancellationToken).ConfigureAwait(false);
        return Results.Created();
    }

    [WolverineGet("/api/v1/homes/{homeId}/assessments")]
    [ProducesResponseType<PaginatedResponse<HomeAssessmentItemResponse>>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetHomeAssessments(
        string homeId,
        [FromQuery] GetHomeAssessmentsQuerySpec querySpec,
        IHomesApiClient client,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);
        var result = await client.GetHomeAssessmentsAsync(homeId, querySpec, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }
}
