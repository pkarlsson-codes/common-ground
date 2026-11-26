using CommonGround.API.Clients;
using CommonGround.API.Contracts.Reminders.Requests;
using CommonGround.API.Contracts.Reminders.Responses;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace CommonGround.Management.Gateway.Endpoints;

public static class RemindersEndpoints
{
    [WolverinePost("/api/v1/reminders")]
    [ProducesResponseType<SetReminderResponse>(StatusCodes.Status201Created)]
    public static async Task<IResult> SetReminder(
        SetReminderRequest request,
        IRemindersApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var result = await client.SetReminderAsync(request, cancellationToken).ConfigureAwait(false);
        return Results.Created($"/api/v1/reminders/{result.ReminderId}", result);
    }

    [WolverineGet("/api/v1/reminders")]
    [ProducesResponseType<ReminderListResponse>(StatusCodes.Status200OK)]
    public static async Task<IResult> ListReminders(IRemindersApiClient client, CancellationToken cancellationToken)
        => Results.Ok(await client.ListRemindersAsync(cancellationToken).ConfigureAwait(false));

    [WolverinePost("/api/v1/reminders/{reminderId}/done")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> MarkReminderDone(
        string reminderId,
        IRemindersApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reminderId);
        await client.MarkReminderDoneAsync(reminderId, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }
}
