namespace CommonGround.API.Contracts.Reminders.Responses;

/// <summary>The ID of a reminder just set.</summary>
/// <param name="ReminderId">The reminder's ID.</param>
public sealed record SetReminderResponse(string ReminderId);

/// <summary>A reminder.</summary>
/// <param name="Id">The reminder's ID.</param>
/// <param name="RemindOn">The day it starts showing on Today.</param>
/// <param name="Note">What to do.</param>
/// <param name="Section">The Today section it shows in, or none for Today's Reminders section.</param>
/// <param name="SetBy">The ID of the staff user who set it.</param>
/// <param name="SetAt">When it was set.</param>
/// <param name="IsDone">Whether someone has marked it done.</param>
/// <param name="DoneBy">The ID of the staff user who marked it done, once someone has.</param>
/// <param name="DoneAt">When it was marked done, once it has been.</param>
public sealed record ReminderResponse(
    string Id,
    DateOnly RemindOn,
    string Note,
    string? Section,
    string SetBy,
    DateTimeOffset SetAt,
    bool IsDone,
    string? DoneBy,
    DateTimeOffset? DoneAt);

/// <summary>The trust's reminders, by where they stand.</summary>
/// <param name="Today">The trust's today.</param>
/// <param name="Showing">Showing on Today: the date has come and no one has marked them done. Oldest date first.</param>
/// <param name="Upcoming">Not yet showing: the date is after today. Soonest first.</param>
/// <param name="Done">Marked done in the last 30 days. Most recent first.</param>
public sealed record ReminderListResponse(
    DateOnly Today,
    IReadOnlyList<ReminderResponse> Showing,
    IReadOnlyList<ReminderResponse> Upcoming,
    IReadOnlyList<ReminderResponse> Done);
