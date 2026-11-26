using CommonGround.API.Contracts.Reminders.Requests;
using CommonGround.API.Contracts.Reminders.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.Extensions.Logging;

namespace CommonGround.API.Clients;

/// <summary>HTTP client for the Reminders API endpoints.</summary>
public sealed class RemindersApiClient(
    IHttpClientFactory httpClientFactory,
    ILogger<RemindersApiClient> logger) : BaseApiClient(httpClientFactory.CreateClient(nameof(RemindersApiClient)), logger), IRemindersApiClient
{
    /// <inheritdoc/>
    public async Task<SetReminderResponse> SetReminderAsync(SetReminderRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await PostAsync<SetReminderRequest, SetReminderResponse>("api/reminders", request, ct).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<ReminderListResponse> ListRemindersAsync(CancellationToken ct = default)
        => await GetAsync<ReminderListResponse>("api/reminders", ct).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task MarkReminderDoneAsync(string reminderId, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reminderId);
        await PostAsync($"api/reminders/{reminderId}/done", ct).ConfigureAwait(false);
    }
}
