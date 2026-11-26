using CommonGround.Reminders.Application.ReadModels;
using CommonGround.SharedKernel.Domain.Services;
using Marten;

namespace CommonGround.Reminders.Application.Queries;

/// <summary>Lists the trust's reminders for the Reminders page.</summary>
/// <param name="Today">The trust's today.</param>
public sealed record ListReminders(DateOnly Today);

/// <summary>The trust's reminders, by where they stand.</summary>
/// <param name="Showing">Showing on Today: the date has come and no one has marked them done. Oldest date first.</param>
/// <param name="Upcoming">Not yet showing: the date is after today. Soonest first.</param>
/// <param name="Done">Marked done in the last <see cref="ListRemindersHandler.DoneDays"/> days. Most recent first.</param>
public sealed record ReminderList(
    IReadOnlyList<ReminderView> Showing,
    IReadOnlyList<ReminderView> Upcoming,
    IReadOnlyList<ReminderView> Done);

/// <summary>Handles <see cref="ListReminders"/>.</summary>
public static class ListRemindersHandler
{
    /// <summary>How many days back the done list goes.</summary>
    public const int DoneDays = 30;

    /// <summary>Loads the reminders not done, and those done in the last <see cref="DoneDays"/> days.</summary>
    /// <param name="request">The query.</param>
    /// <param name="session">The session.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The reminders.</returns>
    public static async Task<ReminderList> Handle(ListReminders request, IQuerySession session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var doneSince = TimeProviderContext.Current.UtcNow.AddDays(-DoneDays);
        var reminders = await session.Query<ReminderView>()
            .Where(r => !r.IsDone || r.DoneAt >= doneSince)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var open = reminders.Where(r => !r.IsDone).OrderBy(r => r.RemindOn).ThenBy(r => r.SetAt).ToList();

        return new ReminderList(
            [.. open.Where(r => r.RemindOn <= request.Today)],
            [.. open.Where(r => r.RemindOn > request.Today)],
            [.. reminders.Where(r => r.IsDone).OrderByDescending(r => r.DoneAt)]);
    }
}
