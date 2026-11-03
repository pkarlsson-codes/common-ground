namespace CommonGround.SharedKernel.Application.WorkItems;

/// <summary>
/// Something that needs a staff member: a record waiting on them, or a date coming up. Each context derives
/// its own from its read models; Today lists them all.
/// </summary>
/// <param name="Area">The part of the CLT's work it belongs to; one of <see cref="WorkArea"/>.</param>
/// <param name="Kind">What is waiting, e.g. <c>application-awaiting-decision</c>.</param>
/// <param name="RecordType">The kind of record it is about, e.g. <c>application</c>.</param>
/// <param name="RecordId">The record's ID.</param>
/// <param name="Label">The name staff know the record by.</param>
/// <param name="Detail">One line saying what is waiting.</param>
/// <param name="DueDate">The date it must be done by, if it has one.</param>
/// <param name="WaitingSince">The date it started waiting, or became due soon.</param>
/// <param name="Actions">
/// What staff can do to fix it, as keys such as <c>approve-application</c>, in the order to offer them; none
/// when opening the record is all there is.
/// </param>
public sealed record WorkItem(
    string Area,
    string Kind,
    string RecordType,
    string RecordId,
    string Label,
    string Detail,
    DateOnly? DueDate,
    DateOnly WaitingSince,
    IReadOnlyList<string> Actions);

/// <summary>The areas work items belong to, in the order the CLT's work runs.</summary>
public static class WorkArea
{
    /// <summary>Households, their applications and readiness.</summary>
    public const string Applicants = "applicants";

    /// <summary>Choosing a household for a home.</summary>
    public const string Selections = "selections";

    /// <summary>Purchases from selection to closing.</summary>
    public const string Purchases = "purchases";

    /// <summary>Records such as documents, outside any one area.</summary>
    public const string Records = "records";

    /// <summary>Every area, in work order.</summary>
    public static IReadOnlyList<string> All { get; } = [Applicants, Selections, Purchases, Records];
}

/// <summary>
/// A context's answer to a work-items query. Wrapped rather than returned as a bare list, which Wolverine
/// would take for cascading messages.
/// </summary>
/// <param name="Items">The context's work items, in no particular order.</param>
public sealed record WorkItemList(IReadOnlyList<WorkItem> Items);
