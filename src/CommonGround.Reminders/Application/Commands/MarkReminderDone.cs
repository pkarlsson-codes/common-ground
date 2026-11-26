using CommonGround.Reminders.Domain.Reminders;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.Reminders.Application.Commands;

/// <summary>Marks a reminder done, so it leaves Today.</summary>
/// <param name="ReminderId">The reminder.</param>
/// <param name="DoneBy">The staff user marking it done.</param>
public sealed record MarkReminderDone(ReminderId ReminderId, UserId DoneBy);

/// <summary>Handles <see cref="MarkReminderDone"/>.</summary>
public static class MarkReminderDoneHandler
{
    /// <summary>Marks the reminder done, unless someone already has.</summary>
    /// <param name="command">The command.</param>
    /// <param name="reminder">The reminder.</param>
    /// <returns>The events decided.</returns>
    public static IEnumerable<object> Handle(MarkReminderDone command, [WriteAggregate] Reminder reminder)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(reminder);

        return reminder.MarkDone(command.DoneBy) is { } done ? [done] : [];
    }
}
