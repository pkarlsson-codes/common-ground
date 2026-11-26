using CommonGround.Reminders.Application.ReadModels;
using CommonGround.Reminders.Application.WorkItems;
using CommonGround.SharedKernel.Application.WorkItems;
using Marten;

namespace CommonGround.Reminders.Application.Queries;

/// <summary>Returns the reminders showing on Today: those whose date has come and that no one has marked done.</summary>
/// <param name="Today">The trust's today.</param>
public sealed record GetRemindersWorkItems(DateOnly Today);

/// <summary>Handles <see cref="GetRemindersWorkItems"/>.</summary>
public static class GetRemindersWorkItemsHandler
{
    /// <summary>Loads the reminders showing today and makes their work items.</summary>
    /// <param name="request">The query.</param>
    /// <param name="session">The session.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The work items.</returns>
    public static async Task<WorkItemList> Handle(GetRemindersWorkItems request, IQuerySession session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var showing = await session.Query<ReminderView>()
            .Where(r => !r.IsDone && r.RemindOn <= request.Today)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new WorkItemList([.. showing.Select(RemindersWorkItems.ForReminder)]);
    }
}
