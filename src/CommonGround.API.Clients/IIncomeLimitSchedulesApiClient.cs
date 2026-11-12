using CommonGround.API.Contracts.HomeBuyers.Requests;
using CommonGround.API.Contracts.HomeBuyers.Responses;

namespace CommonGround.API.Clients;

/// <summary>HTTP client for income limit schedule API endpoints.</summary>
public interface IIncomeLimitSchedulesApiClient
{
    /// <summary>Gets the trust's income limit schedules.</summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The schedules, newest effective date first, with the one in effect marked.</returns>
    Task<IReadOnlyList<IncomeLimitScheduleResponse>> GetIncomeLimitSchedulesAsync(CancellationToken cancellationToken);

    /// <summary>Enters an income limit schedule.</summary>
    /// <param name="request">The schedule.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The new schedule's ID.</returns>
    Task<EnterIncomeLimitScheduleResponse> EnterIncomeLimitScheduleAsync(IncomeLimitScheduleRequest request, CancellationToken cancellationToken);

    /// <summary>Corrects an income limit schedule.</summary>
    /// <param name="scheduleId">The schedule's ID.</param>
    /// <param name="request">The corrected schedule.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that completes when the schedule is corrected.</returns>
    Task CorrectIncomeLimitScheduleAsync(string scheduleId, IncomeLimitScheduleRequest request, CancellationToken cancellationToken);
}
