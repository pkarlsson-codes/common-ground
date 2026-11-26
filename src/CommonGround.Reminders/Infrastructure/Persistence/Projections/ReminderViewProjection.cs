using CommonGround.Reminders.Application.ReadModels;
using CommonGround.Reminders.Domain.Reminders.Events;
using Marten.Events.Aggregation;

namespace CommonGround.Reminders.Infrastructure.Persistence.Projections;

/// <summary>Builds <see cref="ReminderView"/> from Reminder events.</summary>
internal sealed class ReminderViewProjection : SingleStreamProjection<ReminderView, string>
{
    public static ReminderView Create(ReminderSet e) =>
        new()
        {
            Id = e.AggregateId,
            RemindOn = e.RemindOn,
            Note = e.Note,
            Section = e.Section,
            SetBy = e.SetBy,
            SetAt = e.OccurredAt,
        };

    public static void Apply(ReminderView view, ReminderMarkedDone e)
    {
        view.IsDone = true;
        view.DoneBy = e.DoneBy;
        view.DoneAt = e.OccurredAt;
    }
}
