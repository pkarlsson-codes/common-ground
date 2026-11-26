using CommonGround.API.Contracts.Reminders.Requests;
using CommonGround.API.Contracts.Reminders.Responses;

namespace CommonGround.API.Clients;

/// <summary>HTTP client for the Reminders API endpoints.</summary>
public interface IRemindersApiClient
{
    /// <summary>Sets a reminder for the trust.</summary>
    /// <param name="request">The reminder.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The new reminder's ID.</returns>
    Task<SetReminderResponse> SetReminderAsync(SetReminderRequest request, CancellationToken ct = default);

    /// <summary>Lists the trust's reminders: showing, upcoming, and done in the last 30 days.</summary>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The reminders.</returns>
    Task<ReminderListResponse> ListRemindersAsync(CancellationToken ct = default);

    /// <summary>Marks a reminder done.</summary>
    /// <param name="reminderId">The reminder's ID.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A task.</returns>
    Task MarkReminderDoneAsync(string reminderId, CancellationToken ct = default);
}
