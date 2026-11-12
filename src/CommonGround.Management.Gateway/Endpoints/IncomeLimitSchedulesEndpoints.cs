using CommonGround.API.Clients;
using CommonGround.API.Contracts.HomeBuyers.Requests;
using CommonGround.API.Contracts.HomeBuyers.Responses;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace CommonGround.Management.Gateway.Endpoints;

public static class IncomeLimitSchedulesEndpoints
{
    [WolverineGet("/api/v1/income-limit-schedules")]
    [ProducesResponseType<IReadOnlyList<IncomeLimitScheduleResponse>>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetIncomeLimitSchedules(
        IIncomeLimitSchedulesApiClient client,
        CancellationToken cancellationToken)
    {
        var result = await client.GetIncomeLimitSchedulesAsync(cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverinePost("/api/v1/income-limit-schedules")]
    [ProducesResponseType<EnterIncomeLimitScheduleResponse>(StatusCodes.Status201Created)]
    public static async Task<IResult> EnterIncomeLimitSchedule(
        IncomeLimitScheduleRequest request,
        IIncomeLimitSchedulesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var result = await client.EnterIncomeLimitScheduleAsync(request, cancellationToken).ConfigureAwait(false);
        return Results.Created($"/api/v1/income-limit-schedules/{result.ScheduleId}", result);
    }

    [WolverinePut("/api/v1/income-limit-schedules/{scheduleId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> CorrectIncomeLimitSchedule(
        string scheduleId,
        IncomeLimitScheduleRequest request,
        IIncomeLimitSchedulesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scheduleId);
        ArgumentNullException.ThrowIfNull(request);
        await client.CorrectIncomeLimitScheduleAsync(scheduleId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }
}
