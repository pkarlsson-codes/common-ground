using CommonGround.Homeownership.Domain.HeirTransfers;
using Marten;

namespace CommonGround.Homeownership.Application.Services;

/// <summary>
/// Finds the income limit a home carries and the schedule that sets it. The home's limit lives in PropertyRegistry and
/// the schedules in HomeBuyers; the API project reads both.
/// </summary>
public interface IHomeIncomeLimits
{
    /// <summary>
    /// Finds the home's limit under the schedule in effect on a day. Returns null when the home has no income limit.
    /// Refused when the home has a limit and no schedule in effect on that day has its level.
    /// </summary>
    /// <param name="session">The trust's session.</param>
    /// <param name="homeId">The home.</param>
    /// <param name="day">The day whose schedule applies.</param>
    /// <param name="cancellationToken">Cancels the lookup.</param>
    Task<HeirIncomeLimit?> FindAsync(IQuerySession session, string homeId, DateOnly day, CancellationToken cancellationToken);
}
