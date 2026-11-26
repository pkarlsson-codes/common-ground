namespace CommonGround.API.Contracts.Reminders.Requests;

/// <summary>Request to set a reminder for the trust.</summary>
/// <param name="RemindOn">The day it starts showing on Today; not before the trust's today.</param>
/// <param name="Note">What to do; not empty, at most 500 characters.</param>
/// <param name="Section">
/// The Today section it shows in: applicants, selections, purchases, stewardship or records. None puts it in Today's
/// Reminders section.
/// </param>
public sealed record SetReminderRequest(DateOnly RemindOn, string Note, string? Section);
