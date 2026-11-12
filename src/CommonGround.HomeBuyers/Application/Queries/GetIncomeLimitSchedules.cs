using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.SharedKernel.Domain.Services;
using Marten;

namespace CommonGround.HomeBuyers.Application.Queries;

/// <summary>Returns every income limit schedule the trust has entered.</summary>
public sealed record GetIncomeLimitSchedules;

/// <summary>The trust's schedules and which one is in effect today.</summary>
/// <param name="Schedules">Every schedule, latest effective date first.</param>
/// <param name="InEffectScheduleId">The schedule in effect on the trust's today, if any.</param>
public sealed record IncomeLimitSchedules(IReadOnlyList<IncomeLimitScheduleView> Schedules, string? InEffectScheduleId);

/// <summary>Handles <see cref="GetIncomeLimitSchedules"/>.</summary>
public static class GetIncomeLimitSchedulesHandler
{
    /// <summary>Lists the schedules and marks the one in effect on the trust's today.</summary>
    public static async Task<IncomeLimitSchedules> Handle(
        GetIncomeLimitSchedules request,
        IQuerySession session,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(trustClock);

        var schedules = await session.Query<IncomeLimitScheduleView>()
            .OrderByDescending(v => v.EffectiveFrom)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var today = await trustClock.TodayAsync(session.TenantId, cancellationToken).ConfigureAwait(false);
        var inEffect = IncomeLimitScheduleView.InEffectOn(schedules, today);

        return new IncomeLimitSchedules(schedules, inEffect?.Id);
    }
}
