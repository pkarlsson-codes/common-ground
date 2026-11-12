using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.IncomeLimitSchedules;
using CommonGround.SharedKernel.Domain;
using Marten;
using Wolverine.Marten;

namespace CommonGround.HomeBuyers.Application.Commands;

/// <summary>Enters a published income limit schedule.</summary>
/// <param name="ScheduleId">The new schedule's ID, chosen by the caller.</param>
/// <param name="EffectiveFrom">The date it applies from.</param>
/// <param name="Levels">Its levels.</param>
/// <param name="EnteredBy">The staff user entering it.</param>
public sealed record EnterIncomeLimitSchedule(
    IncomeLimitScheduleId ScheduleId,
    DateOnly EffectiveFrom,
    IReadOnlyList<IncomeLimitLevel> Levels,
    UserId EnteredBy);

/// <summary>The trust's schedule already effective from a date, if there is one.</summary>
/// <param name="ScheduleId">That schedule's ID, or <see langword="null"/> when the date is free.</param>
public sealed record ScheduleOnDate(IncomeLimitScheduleId? ScheduleId)
{
    /// <summary>Finds the trust's schedule effective from <paramref name="effectiveFrom"/>, other than <paramref name="exceptScheduleId"/>.</summary>
    internal static async Task<ScheduleOnDate> FindAsync(
        IQuerySession session,
        DateOnly effectiveFrom,
        IncomeLimitScheduleId? exceptScheduleId,
        CancellationToken cancellationToken)
    {
        var onDate = session.Query<IncomeLimitScheduleView>().Where(v => v.EffectiveFrom == effectiveFrom);
        if (exceptScheduleId is { Value: var except })
        {
            onDate = onDate.Where(v => v.Id != except);
        }

        var id = await onDate
            .Select(v => v.Id)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return new ScheduleOnDate(EntityIds.CreateOrNull<IncomeLimitScheduleId>(id));
    }
}

/// <summary>Handles <see cref="EnterIncomeLimitSchedule"/>.</summary>
public static class EnterIncomeLimitScheduleHandler
{
    /// <summary>Finds a schedule already effective from the same date.</summary>
    public static Task<ScheduleOnDate> LoadAsync(
        EnterIncomeLimitSchedule command,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        return ScheduleOnDate.FindAsync(session, command.EffectiveFrom, exceptScheduleId: null, cancellationToken);
    }

    /// <summary>Starts the schedule's stream.</summary>
    public static IStartStream Handle(EnterIncomeLimitSchedule command, ScheduleOnDate onDate)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(onDate);

        var entered = IncomeLimitSchedule.Enter(
            command.ScheduleId, command.EffectiveFrom, command.Levels, onDate.ScheduleId, command.EnteredBy);
        return MartenOps.StartStream<IncomeLimitSchedule>(command.ScheduleId.Value, entered);
    }
}
