using CommonGround.Documents.Application.WorkItems;
using CommonGround.HomeBuyers.Application.WorkItems;
using CommonGround.Homeownership.Application.WorkItems;
using CommonGround.PropertyRegistry.Application.WorkItems;
using CommonGround.Reminders.Application.WorkItems;
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
    /// The kinds of work in the order a staff member works them inside a Today section, first to last. Each kind
    /// appears once; a section's kinds are listed together. Kinds not listed go after the listed ones.
    /// </summary>
    private static readonly string[] KindOrder =
    [

        // A reminder leads whatever section it is in: staff set it for that day on purpose.
        RemindersWorkItems.Reminder,

        // Applicants
        HomeBuyersWorkItems.ApplicationAwaitingDecision,
        HomeBuyersWorkItems.QualificationToConfirm,
        HomeBuyersWorkItems.ReadinessStepMissing,
        HomeBuyersWorkItems.PrequalificationExpiring,

        // Selections
        PropertyRegistryWorkItems.HomeAvailable,
        HomeBuyersWorkItems.IncomeLimitsMissing,
        HomeBuyersWorkItems.IncomeLimitsStale,

        // Purchases
        HomeownershipWorkItems.ClosingScheduled,
        HomeownershipWorkItems.PurchaseStalled,
        HomeownershipWorkItems.PurchaseStepWaiting,

        // Stewardship: lease deadlines and legal consequences first, bookkeeping and routine renewals last.
        HomeownershipWorkItems.ResaleDecisionDue,
        HomeownershipWorkItems.LeaseDefaultUncured,
        HomeownershipWorkItems.MortgageRequestPending,
        HomeownershipWorkItems.ImprovementRequestWaiting,
        HomeownershipWorkItems.InsuranceShortOfLease,
        HomeownershipWorkItems.InsuranceLapsed,
        HomeownershipWorkItems.LeaseFeeNoticeDue,
        HomeownershipWorkItems.ResaleClosingWaiting,
        HomeownershipWorkItems.MortgageLoanClosingUnrecorded,
        HomeownershipWorkItems.ResalePriceStatementDue,
        HomeownershipWorkItems.InsuranceEndingSoon,

        // Records
        DocumentsWorkItems.DocumentExpiring,
    ];

    /// <summary>
    /// Orders items by the position of their kind, so within a section a lease default sorts before a lapsed
    /// insurance certificate however old the certificate's date is. Within a kind, items with a due date come
    /// before those without, then the earliest due date, then the longest wait; ties fall back to the label so the
    /// order is stable. Positions run across sections, so the result only has a meaning inside one section.
    /// </summary>
    /// <param name="items">The items to order.</param>
    /// <returns>The items, most urgent first.</returns>
    public static IReadOnlyList<WorkItem> Order(IEnumerable<WorkItem> items) =>
        [.. items
            .OrderBy(i => PositionOf(i.Kind))
            .ThenBy(i => i.DueDate is null)
            .ThenBy(i => i.DueDate)
            .ThenBy(i => i.WaitingSince ?? DateOnly.MaxValue)
            .ThenBy(i => i.Label, StringComparer.Ordinal)];

    /// <summary>Where a kind sorts: 0 is first. Kinds not listed sort after every listed kind.</summary>
    /// <param name="kind">The work item's kind.</param>
    /// <returns>The kind's position.</returns>
    public static int PositionOf(string kind)
    {
        var index = Array.IndexOf(KindOrder, kind);
        return index < 0 ? KindOrder.Length : index;
    }

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
