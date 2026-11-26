using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Reminders.Domain.Reminders.Events;

/// <summary>Raised when a member of the trust marks a reminder done. When is the event's <c>OccurredAt</c>.</summary>
/// <param name="AggregateId">The reminder's ID.</param>
/// <param name="DoneBy">The staff user who marked it done.</param>
public sealed record ReminderMarkedDone(string AggregateId, string DoneBy) : DomainEvent(AggregateId);
