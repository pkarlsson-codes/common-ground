using CommonGround.SharedKernel.Application.WorkItems;

namespace CommonGround.API.Today;

/// <summary>Orders work items by urgency and says how urgent each is.</summary>
public static class WorkUrgency
{
    /// <summary>How far ahead a due date counts as soon.</summary>
    public const int SoonDays = 7;

    /// <summary>Past its due date.</summary>
    public const string Overdue = "overdue";

    /// <summary>Due within <see cref="SoonDays"/>.</summary>
    public const string Soon = "soon";

    /// <summary>Due later, or with no due date.</summary>
    public const string Plain = "plain";

    /// <summary>
    /// Orders items overdue first, then due soonest, then waiting longest. Items with a due date come before
    /// those without; ties fall back to the label so the order is stable.
    /// </summary>
    /// <param name="items">The items to order.</param>
    /// <returns>The items, most urgent first.</returns>
    public static IReadOnlyList<WorkItem> Order(IEnumerable<WorkItem> items) =>
        [.. items
            .OrderBy(i => i.DueDate is null)
            .ThenBy(i => i.DueDate)
            .ThenBy(i => i.WaitingSince)
            .ThenBy(i => i.Label, StringComparer.Ordinal)];

    /// <summary>How urgent an item is on <paramref name="today"/>.</summary>
    /// <param name="item">The item.</param>
    /// <param name="today">The trust's today.</param>
    /// <returns><see cref="Overdue"/>, <see cref="Soon"/> or <see cref="Plain"/>.</returns>
    public static string UrgencyOf(WorkItem item, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(item);

        return item.DueDate switch
        {
            { } due when due < today => Overdue,
            { } due when due <= today.AddDays(SoonDays) => Soon,
            _ => Plain,
        };
    }
}
