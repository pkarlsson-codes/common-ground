using CommonGround.API.Contracts.HomeBuyers.Requests;
using CommonGround.API.Contracts.HomeBuyers.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.Extensions.Logging;

namespace CommonGround.API.Clients;

/// <summary>HTTP client for income limit schedule API endpoints.</summary>
public sealed class IncomeLimitSchedulesApiClient(
    IHttpClientFactory httpClientFactory,
    ILogger<IncomeLimitSchedulesApiClient> logger)
    : BaseApiClient(httpClientFactory.CreateClient(nameof(IncomeLimitSchedulesApiClient)), logger), IIncomeLimitSchedulesApiClient
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<IncomeLimitScheduleResponse>> GetIncomeLimitSchedulesAsync(CancellationToken cancellationToken)
        => await GetAsync<List<IncomeLimitScheduleResponse>>("api/income-limit-schedules", cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<EnterIncomeLimitScheduleResponse> EnterIncomeLimitScheduleAsync(
        IncomeLimitScheduleRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await PostAsync<IncomeLimitScheduleRequest, EnterIncomeLimitScheduleResponse>(
            "api/income-limit-schedules", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task CorrectIncomeLimitScheduleAsync(
        string scheduleId, IncomeLimitScheduleRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scheduleId);
        ArgumentNullException.ThrowIfNull(request);
        await PutAsync($"api/income-limit-schedules/{scheduleId}", request, cancellationToken).ConfigureAwait(false);
    }
}
