using CommonGround.API.Contracts.Today.Responses;

namespace CommonGround.API.Clients;

/// <summary>HTTP client for the Today API endpoint.</summary>
public interface ITodayApiClient
{
    /// <summary>Gets what needs attention today.</summary>
    /// <param name="areas">The areas to list items for; all areas when none are given.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The items in those areas, most urgent first, and the counts across every area.</returns>
    Task<TodayResponse> GetTodayAsync(IReadOnlyList<string>? areas, CancellationToken ct = default);
}
