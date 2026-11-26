using CommonGround.SharedKernel.Domain.Exceptions;

namespace CommonGround.Reminders.Application.Errors;

/// <summary>The business rules the Reminders context enforces, by stable code.</summary>
public static class BusinessRuleViolation
{
    /// <summary>Rules on a reminder.</summary>
    public static class Reminder
    {
        /// <summary>Rules on setting a reminder.</summary>
        public static class Setting
        {
            /// <summary>The note is empty.</summary>
            /// <returns>The violation.</returns>
            public static BusinessRuleViolationException NoteRequired()
                => new("reminders.reminder.set.note_required");

            /// <summary>The note is longer than a reminder holds.</summary>
            /// <param name="length">The note's length.</param>
            /// <param name="maxLength">The most a note can hold.</param>
            /// <returns>The violation.</returns>
            public static BusinessRuleViolationException NoteTooLong(int length, int maxLength)
                => new(
                    "reminders.reminder.set.note_too_long",
                    new Dictionary<string, object?>
                    {
                        ["length"] = length,
                        ["maxLength"] = maxLength,
                    });

            /// <summary>The date is before the trust's today.</summary>
            /// <param name="remindOn">The date asked for.</param>
            /// <param name="today">The trust's today.</param>
            /// <returns>The violation.</returns>
            public static BusinessRuleViolationException RemindOnInPast(DateOnly remindOn, DateOnly today)
                => new(
                    "reminders.reminder.set.remind_on_in_past",
                    new Dictionary<string, object?>
                    {
                        ["remindOn"] = remindOn,
                        ["today"] = today,
                    });

            /// <summary>The section isn't one of Today's.</summary>
            /// <param name="section">The section asked for.</param>
            /// <returns>The violation.</returns>
            public static BusinessRuleViolationException SectionUnknown(string section)
                => new(
                    "reminders.reminder.set.section_unknown",
                    new Dictionary<string, object?>
                    {
                        ["section"] = section,
                    });
        }
    }
}
