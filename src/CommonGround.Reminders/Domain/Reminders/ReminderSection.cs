using CommonGround.SharedKernel.Application.WorkItems;

namespace CommonGround.Reminders.Domain.Reminders;

/// <summary>
/// The Today sections a reminder can be set in, named as Today names them. A reminder with no section shows in
/// Today's Reminders section, which is not one of these.
/// </summary>
public static class ReminderSection
{
    /// <summary>Every section a reminder can be set in, in Today's order.</summary>
    public static IReadOnlyList<string> All { get; } = [.. WorkArea.All.Where(a => a != WorkArea.Reminders)];

    /// <summary>Whether <paramref name="section"/> is one a reminder can be set in.</summary>
    /// <param name="section">The section's name.</param>
    /// <returns><see langword="true"/> when it is one of <see cref="All"/>.</returns>
    public static bool IsKnown(string section) => All.Contains(section, StringComparer.Ordinal);
}
