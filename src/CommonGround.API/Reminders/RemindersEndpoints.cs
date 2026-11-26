using CommonGround.API.Contracts.Reminders.Requests;
using CommonGround.API.Contracts.Reminders.Responses;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.Reminders.Application.Commands;
using CommonGround.Reminders.Application.Queries;
using CommonGround.Reminders.Application.ReadModels;
using CommonGround.Reminders.Domain.Reminders;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Infrastructure.API;
using Wolverine;
using Wolverine.Http;

namespace CommonGround.API.Reminders;

/// <summary>Reminders staff set for the trust, which show on Today from their date until someone marks them done.</summary>
public static class RemindersEndpoints
{
    [WolverinePost("/api/reminders")]
    public static async Task<IResult> SetReminder(
        SetReminderRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var id = ReminderId.Generate();

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new SetReminder(id, request.RemindOn, request.Note, request.Section, httpContext.GetUserId()),
            cancellationToken);

        return Results.Created($"/api/reminders/{id.Value}", new SetReminderResponse(id.Value));
    }

    [WolverineGet("/api/reminders")]
    public static async Task<IResult> ListReminders(IMessageBus bus, ITrustClock trustClock, CancellationToken cancellationToken)
    {
        var tenantId = bus.TenantId!;
        var today = await trustClock.TodayAsync(tenantId, cancellationToken).ConfigureAwait(false);

        var list = await bus.InvokeForTenantAsync<ReminderList>(tenantId, new ListReminders(today), cancellationToken).ConfigureAwait(false);

        return Results.Ok(new ReminderListResponse(
            today,
            [.. list.Showing.Select(ToResponse)],
            [.. list.Upcoming.Select(ToResponse)],
            [.. list.Done.Select(ToResponse)]));
    }

    [WolverinePost("/api/reminders/{reminderId}/done")]
    public static async Task<IResult> MarkReminderDone(
        string reminderId,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reminderId);

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new MarkReminderDone(ReminderId.Create(reminderId), httpContext.GetUserId()),
            cancellationToken);

        return Results.NoContent();
    }

    private static ReminderResponse ToResponse(ReminderView r) =>
        new(r.Id, r.RemindOn, r.Note, r.Section, r.SetBy, r.SetAt, r.IsDone, r.DoneBy, r.DoneAt);
}
