using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.IncomeLimitSchedules.Events;
using Marten.Events.Aggregation;

namespace CommonGround.HomeBuyers.Infrastructure.Persistence.Projections;

/// <summary>Builds <see cref="IncomeLimitScheduleView"/> from IncomeLimitSchedule events.</summary>
internal sealed class IncomeLimitScheduleViewProjection : SingleStreamProjection<IncomeLimitScheduleView, string>
{
    public static IncomeLimitScheduleView Create(IncomeLimitScheduleEntered e) =>
        new()
        {
            Id = e.AggregateId,
            EffectiveFrom = e.EffectiveFrom,
            Levels = ToEntries(e.Levels),
            EnteredBy = e.EnteredById,
            EnteredAt = e.OccurredAt,
        };

    public static void Apply(IncomeLimitScheduleView view, IncomeLimitScheduleCorrected e)
    {
        view.EffectiveFrom = e.EffectiveFrom;
        view.Levels = ToEntries(e.Levels);
        view.CorrectedBy = e.CorrectedById;
        view.CorrectedAt = e.OccurredAt;
    }

    private static List<IncomeLimitLevelEntry> ToEntries(IEnumerable<EnteredIncomeLimitLevel> levels) =>
        [.. levels.Select(l => new IncomeLimitLevelEntry
        {
            PercentOfAmi = l.PercentOfAmi,
            Limits = [.. l.Limits.Select(m => m.Amount)],
        })];
}
