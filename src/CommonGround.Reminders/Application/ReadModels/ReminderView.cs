namespace CommonGround.Reminders.Application.ReadModels;

/// <summary>A reminder as Today and the Reminders page list it.</summary>
public sealed class ReminderView
{
    /// <summary>Gets or sets the reminder's ID.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the day it starts showing on Today.</summary>
    public DateOnly RemindOn { get; set; }

    /// <summary>Gets or sets what to do.</summary>
    public string Note { get; set; } = string.Empty;

    /// <summary>Gets or sets the Today section it shows in, or none for Today's Reminders section.</summary>
    public string? Section { get; set; }

    /// <summary>Gets or sets the staff user who set it.</summary>
    public string SetBy { get; set; } = string.Empty;

    /// <summary>Gets or sets when it was set.</summary>
    public DateTimeOffset SetAt { get; set; }

    /// <summary>Gets or sets a value indicating whether someone has marked it done.</summary>
    public bool IsDone { get; set; }

    /// <summary>Gets or sets the staff user who marked it done, once someone has.</summary>
    public string? DoneBy { get; set; }

    /// <summary>Gets or sets when it was marked done, once it has been.</summary>
    public DateTimeOffset? DoneAt { get; set; }
}
