namespace CommonGround.API.Contracts.Today.Responses;

/// <summary>What needs attention today, most urgent first, and how many items each area and kind has.</summary>
/// <param name="Today">The trust's today.</param>
/// <param name="TotalCount">How many items there are across every area, including those filtered out.</param>
/// <param name="Items">The items in the requested areas, most urgent first.</param>
/// <param name="AreaCounts">How many items each area has, every area in work order, including those filtered out.</param>
/// <param name="KindCounts">How many items each kind has, including those filtered out.</param>
public sealed record TodayResponse(
    DateOnly Today,
    int TotalCount,
    IReadOnlyList<WorkItemResponse> Items,
    IReadOnlyList<WorkCountResponse> AreaCounts,
    IReadOnlyList<WorkCountResponse> KindCounts);

/// <summary>Something that needs a staff member.</summary>
/// <param name="Area">The area it belongs to: applicants, selections, purchases or records.</param>
/// <param name="Kind">What is waiting, e.g. <c>application-awaiting-decision</c>.</param>
/// <param name="RecordType">The kind of record it is about, e.g. <c>application</c>.</param>
/// <param name="RecordId">The record's ID.</param>
/// <param name="Label">The name staff know the record by.</param>
/// <param name="Detail">One line saying what is waiting.</param>
/// <param name="DueDate">The date it must be done by, if it has one.</param>
/// <param name="WaitingSince">The date it started waiting, or became due soon; none when the item has no meaningful start.</param>
/// <param name="Actions">What staff can do to fix it, as keys such as <c>approve-application</c>, in the order to offer them.</param>
/// <param name="Urgency">"overdue" when past its due date, "soon" when due within a week, otherwise "plain".</param>
public sealed record WorkItemResponse(
    string Area,
    string Kind,
    string RecordType,
    string RecordId,
    string Label,
    string Detail,
    DateOnly? DueDate,
    DateOnly? WaitingSince,
    IReadOnlyList<string> Actions,
    string Urgency);

/// <summary>How many items an area or kind has.</summary>
/// <param name="Key">The area or kind.</param>
/// <param name="Count">How many items it has.</param>
public sealed record WorkCountResponse(string Key, int Count);
