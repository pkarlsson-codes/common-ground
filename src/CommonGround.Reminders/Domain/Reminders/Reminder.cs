using CommonGround.Reminders.Application.Errors;
using CommonGround.Reminders.Domain.Reminders.Events;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.Reminders.Domain.Reminders;

/// <summary>
/// A dated note set for the trust, so work that has no record behind it shows on Today. It belongs to the trust, not
/// to whoever set it: every member sees it, and any member can mark it done. Before its date it is upcoming; from its
/// date it shows on Today until someone marks it done. It is not a task: it has no owner, status or priority.
/// </summary>
public sealed class Reminder : AggregateRoot<ReminderId>
{
    /// <summary>The most characters a note holds.</summary>
    public const int NoteMaxLength = 500;

#pragma warning disable CS8618 // State is set by Create from the stream's first event.
    private Reminder() { }
#pragma warning restore CS8618

    /// <summary>Gets the day it starts showing on Today.</summary>
    public DateOnly RemindOn { get; private set; }

    /// <summary>Gets a value indicating whether someone has marked it done.</summary>
    public bool IsDone { get; private set; }

    /// <summary>
    /// Sets a reminder. Refused for an empty or overlong note, a date before the trust's today, or a section that
    /// isn't one of Today's. The note is trimmed, and a blank section means none.
    /// </summary>
    /// <param name="id">The new reminder's ID.</param>
    /// <param name="remindOn">The day it starts showing on Today.</param>
    /// <param name="note">What to do.</param>
    /// <param name="section">The Today section it shows in, or none for Today's Reminders section.</param>
    /// <param name="today">The trust's today.</param>
    /// <param name="setBy">The staff user setting it.</param>
    /// <returns>The event that starts the reminder's stream.</returns>
    public static ReminderSet Set(ReminderId id, DateOnly remindOn, string note, string? section, DateOnly today, UserId setBy)
    {
        var trimmed = note?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
            throw BusinessRuleViolation.Reminder.Setting.NoteRequired();

        if (trimmed.Length > NoteMaxLength)
            throw BusinessRuleViolation.Reminder.Setting.NoteTooLong(trimmed.Length, NoteMaxLength);

        if (remindOn < today)
            throw BusinessRuleViolation.Reminder.Setting.RemindOnInPast(remindOn, today);

        var sectionOrNone = string.IsNullOrWhiteSpace(section) ? null : section.Trim();
        if (sectionOrNone is not null && !ReminderSection.IsKnown(sectionOrNone))
            throw BusinessRuleViolation.Reminder.Setting.SectionUnknown(sectionOrNone);

        return new ReminderSet(id.Value, remindOn, trimmed, sectionOrNone, setBy.Value);
    }

    /// <summary>Marks the reminder done, so it leaves Today. Nothing happens if someone already has.</summary>
    /// <param name="doneBy">The staff user marking it done.</param>
    /// <returns>The event, or <see langword="null"/> when it is already done.</returns>
    public ReminderMarkedDone? MarkDone(UserId doneBy) =>
        IsDone ? null : new ReminderMarkedDone(Id.Value, doneBy.Value);

    /// <summary>Starts the reminder from its first event.</summary>
    /// <param name="e">The event.</param>
    /// <returns>The reminder.</returns>
    public static Reminder Create(ReminderSet e)
    {
        ArgumentNullException.ThrowIfNull(e);

        return new Reminder
        {
            Id = ReminderId.Create(e.AggregateId),
            RemindOn = e.RemindOn,
        };
    }

    /// <summary>Records that it is done.</summary>
    /// <param name="e">The event.</param>
    public void Apply(ReminderMarkedDone e) => IsDone = true;
}
