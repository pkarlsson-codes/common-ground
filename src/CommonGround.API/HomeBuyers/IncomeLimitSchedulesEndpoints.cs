using CommonGround.API.Contracts.HomeBuyers.Requests;
using CommonGround.API.Contracts.HomeBuyers.Responses;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.HomeBuyers.Application.Commands;
using CommonGround.HomeBuyers.Application.Queries;
using CommonGround.HomeBuyers.Domain.IncomeLimitSchedules;
using CommonGround.SharedKernel.Domain;
using Wolverine;
using Wolverine.Http;

namespace CommonGround.API.HomeBuyers;

public static class IncomeLimitSchedulesEndpoints
{
    [WolverineGet("/api/income-limit-schedules")]
    public static async Task<IResult> GetIncomeLimitSchedules(
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        var result = await bus.InvokeForTenantAsync<IncomeLimitSchedules>(bus.TenantId!,
            new GetIncomeLimitSchedules(),
            cancellationToken);

        var response = result.Schedules.Select(s => new IncomeLimitScheduleResponse(
            s.Id,
            s.EffectiveFrom,
            s.Id == result.InEffectScheduleId,
            [.. s.Levels.Select(l => new IncomeLimitLevelResponse(l.PercentOfAmi, l.Limits))],
            s.EnteredBy,
            s.EnteredAt,
            s.CorrectedBy,
            s.CorrectedAt));

        return Results.Ok(response);
    }

    [WolverinePost("/api/income-limit-schedules")]
    public static async Task<IResult> EnterIncomeLimitSchedule(
        IncomeLimitScheduleRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var id = IncomeLimitScheduleId.Generate();
        await bus.InvokeForTenantAsync(bus.TenantId!,
            new EnterIncomeLimitSchedule(id, request.EffectiveFrom, ToLevels(request.Levels), httpContext.GetUserId()),
            cancellationToken);

        return Results.Created($"/api/income-limit-schedules/{id.Value}", new EnterIncomeLimitScheduleResponse(id.Value));
    }

    [WolverinePut("/api/income-limit-schedules/{scheduleId}")]
    public static async Task<IResult> CorrectIncomeLimitSchedule(
        string scheduleId,
        IncomeLimitScheduleRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scheduleId);
        ArgumentNullException.ThrowIfNull(request);

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new CorrectIncomeLimitSchedule(
                IncomeLimitScheduleId.Create(scheduleId),
                request.EffectiveFrom,
                ToLevels(request.Levels),
                httpContext.GetUserId()),
            cancellationToken);

        return Results.NoContent();
    }

    private static List<IncomeLimitLevel> ToLevels(IReadOnlyList<IncomeLimitLevelRequest>? levels) =>
        [.. (levels ?? []).Select(l => IncomeLimitLevel.Create(l.PercentOfAmi, [.. (l.Limits ?? []).Select(Money.Create)]))];
}
