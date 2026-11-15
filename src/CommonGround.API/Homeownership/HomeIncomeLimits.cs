using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.Homeownership.Application.Errors;
using CommonGround.Homeownership.Application.Services;
using CommonGround.Homeownership.Domain.HeirTransfers;
using CommonGround.SharedKernel.Domain;
using Marten;

namespace CommonGround.API.Homeownership;

/// <summary>
/// Reads a home's income limit (PropertyRegistry's fact, kept in HomeBuyers' <see cref="HomeFactsView"/>) and the
/// income limit schedules (HomeBuyers). It lives in the API project because Homeownership references neither context.
/// </summary>
internal sealed class HomeIncomeLimits : IHomeIncomeLimits
{
    /// <inheritdoc/>
    public async Task<HeirIncomeLimit?> FindAsync(IQuerySession session, string homeId, DateOnly day, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);

        var home = await session.LoadAsync<HomeFactsView>(homeId, cancellationToken).ConfigureAwait(false);
        if (home?.IncomeLimitPercentOfAmi is not { } percent)
        {
            return null;
        }

        var schedules = await session.Query<IncomeLimitScheduleView>().ToListAsync(cancellationToken).ConfigureAwait(false);
        var inEffect = IncomeLimitScheduleView.InEffectOn(schedules, day);
        var table = inEffect is null
            ? null
            : IncomeLimitTable.FromEntries(
                inEffect.EffectiveFrom,
                inEffect.Levels.Select(l => new CommonGround.SharedKernel.Domain.Events.IncomeLimitLevelEntry(l.PercentOfAmi, [.. l.Limits.Select(Money.Create)])));

        if (table?.LevelFor(percent) is null)
        {
            throw BusinessRuleViolation.HeirTransfer.RecordHeir.IncomeLimitsMissing(percent, day);
        }

        return new HeirIncomeLimit(percent, table);
    }
}
