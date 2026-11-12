using CommonGround.HomeBuyers.Domain.IncomeLimitSchedules;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Extensions;
using JasperFx.Events;
using Marten;

namespace CommonGround.HomeBuyers.Application.Commands;

/// <summary>Replaces a schedule's effective date and levels. Selections already started keep the limits they used.</summary>
/// <param name="ScheduleId">The schedule to correct.</param>
/// <param name="EffectiveFrom">The corrected date it applies from.</param>
/// <param name="Levels">The corrected levels.</param>
/// <param name="CorrectedBy">The staff user correcting it.</param>
public sealed record CorrectIncomeLimitSchedule(
    IncomeLimitScheduleId ScheduleId,
    DateOnly EffectiveFrom,
    IReadOnlyList<IncomeLimitLevel> Levels,
    UserId CorrectedBy);

/// <summary>The schedule to correct, fetched for writing, and any other schedule on the corrected date.</summary>
/// <param name="Schedule">The schedule's stream.</param>
/// <param name="OnDate">Another schedule effective from the corrected date, if any.</param>
public sealed record ScheduleCorrection(IEventStream<IncomeLimitSchedule> Schedule, ScheduleOnDate OnDate);

/// <summary>Handles <see cref="CorrectIncomeLimitSchedule"/>.</summary>
public static class CorrectIncomeLimitScheduleHandler
{
    /// <summary>Fetches the schedule and finds another schedule on the corrected date.</summary>
    public static async Task<ScheduleCorrection> LoadAsync(
        CorrectIncomeLimitSchedule command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(session);

        var schedule = await session.Events
            .FetchForWriting<IncomeLimitSchedule>(command.ScheduleId.Value, cancellationToken)
            .ConfigureAwait(false);
        _ = schedule.Aggregate ?? throw new AggregateNotFoundException(typeof(IncomeLimitSchedule), command.ScheduleId.Value);

        var onDate = await ScheduleOnDate
            .FindAsync(session, command.EffectiveFrom, command.ScheduleId, cancellationToken)
            .ConfigureAwait(false);

        return new ScheduleCorrection(schedule, onDate);
    }

    /// <summary>Corrects the schedule.</summary>
    public static void Handle(CorrectIncomeLimitSchedule command, ScheduleCorrection correction)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(correction);

        correction.Schedule.Decide(s => s.Correct(
            command.EffectiveFrom, command.Levels, correction.OnDate.ScheduleId, command.CorrectedBy));
    }
}
