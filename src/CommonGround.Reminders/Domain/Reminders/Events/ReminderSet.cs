using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Reminders.Domain.Reminders.Events;

/// <summary>Raised when staff set a reminder for the trust. When it was set is the event's <c>OccurredAt</c>.</summary>
/// <param name="AggregateId">The reminder's ID.</param>
/// <param name="RemindOn">The day it starts showing on Today.</param>
/// <param name="Note">What to do, in staff's own words.</param>
/// <param name="Section">The Today section it shows in, or none for Today's Reminders section.</param>
/// <param name="SetBy">The staff user who set it.</param>
public sealed record ReminderSet(
    string AggregateId,
    DateOnly RemindOn,
    string Note,
    string? Section,
    string SetBy) : DomainEvent(AggregateId);
