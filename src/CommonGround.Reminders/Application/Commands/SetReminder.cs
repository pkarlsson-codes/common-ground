using CommonGround.Reminders.Domain.Reminders;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using Marten;
using Wolverine.Marten;

namespace CommonGround.Reminders.Application.Commands;

/// <summary>Sets a reminder for the trust.</summary>
/// <param name="ReminderId">The new reminder's ID, chosen by the caller.</param>
/// <param name="RemindOn">The day it starts showing on Today.</param>
/// <param name="Note">What to do.</param>
/// <param name="Section">The Today section it shows in, or none for Today's Reminders section.</param>
/// <param name="SetBy">The staff user setting it.</param>
public sealed record SetReminder(ReminderId ReminderId, DateOnly RemindOn, string Note, string? Section, UserId SetBy);

/// <summary>Handles <see cref="SetReminder"/>.</summary>
public static class SetReminderHandler
{
    /// <summary>Loads the trust's today.</summary>
    /// <param name="command">The command.</param>
    /// <param name="session">The session.</param>
    /// <param name="trustClock">The trust's clock.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The trust's today.</returns>
    public static Task<DateOnly> LoadAsync(SetReminder command, IDocumentSession session, ITrustClock trustClock, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(trustClock);

        return trustClock.TodayAsync(session.TenantId, cancellationToken);
    }

    /// <summary>Opens the reminder's stream.</summary>
    /// <param name="command">The command.</param>
    /// <param name="today">The trust's today.</param>
    /// <returns>The stream to start.</returns>
    public static IStartStream Handle(SetReminder command, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(command);

        var set = Reminder.Set(command.ReminderId, command.RemindOn, command.Note, command.Section, today, command.SetBy);
        return MartenOps.StartStream<Reminder>(command.ReminderId.Value, set);
    }
}
