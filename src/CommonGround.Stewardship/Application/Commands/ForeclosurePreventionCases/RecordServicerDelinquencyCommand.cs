namespace CommonGround.Stewardship.Application.Commands.ForeclosurePreventionCases;

/// <summary>
/// Command to record servicer delinquency for a foreclosure prevention case.
/// </summary>
/// <param name="ForeclosurePreventionCaseId">The ID of the foreclosure prevention case.</param>
/// <param name="ServicerName">The name of the servicer.</param>
/// <param name="DelinquentMonths">The number of delinquent months.</param>
/// <param name="RecordedDate">The date the delinquency was recorded.</param>
/// <param name="RecordedBy">The user ID of who recorded the delinquency.</param>
public sealed record RecordServicerDelinquencyCommand(
    string ForeclosurePreventionCaseId,
    string ServicerName,
    int DelinquentMonths,
    DateOnly RecordedDate,
    string RecordedBy);
