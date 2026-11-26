using CommonGround.Reminders.Application.ReadModels;
using CommonGround.SharedKernel.Application.WorkItems;

namespace CommonGround.Reminders.Application.WorkItems;

/// <summary>The work items reminders put on Today.</summary>
public static class RemindersWorkItems
{
    /// <summary>A reminder whose date has come and that no one has marked done.</summary>
    public const string Reminder = "reminder";

    /// <summary>The record type a reminder's work item names.</summary>
    public const string RecordType = "reminder";

    /// <summary>Marks the reminder done.</summary>
    public const string MarkDone = "mark-reminder-done";

    /// <summary>
    /// The work item for a reminder showing on Today: in its section, or in the Reminders section when it has none.
    /// Its label is the note. It has no due date, since its date is when it starts showing, not a deadline; it waits
    /// from its date.
    /// </summary>
    /// <param name="reminder">The reminder.</param>
    /// <returns>The work item.</returns>
    public static WorkItem ForReminder(ReminderView reminder)
    {
        ArgumentNullException.ThrowIfNull(reminder);

        return new WorkItem(
            reminder.Section ?? WorkArea.Reminders,
            Reminder,
            RecordType,
            reminder.Id,
            reminder.Note,
            string.Empty,
            null,
            reminder.RemindOn,
            [MarkDone]);
    }
}
