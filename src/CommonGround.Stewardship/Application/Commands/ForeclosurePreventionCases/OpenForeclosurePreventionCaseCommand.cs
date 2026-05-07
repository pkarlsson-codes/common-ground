using CommonGround.SharedKernel.Domain;

namespace CommonGround.Stewardship.Application.Commands.ForeclosurePreventionCases;

/// <summary>Opens a foreclosure prevention case for a servicer delinquency trigger.</summary>
/// <param name="GroundLeaseId">The ground lease associated with the delinquency.</param>
/// <param name="ServicerName">The name of the servicer reporting delinquency.</param>
/// <param name="DelinquentMonths">The number of delinquent months.</param>
/// <param name="RecordedDate">The date the delinquency was recorded.</param>
/// <param name="RecordedBy">The user ID of who recorded it.</param>
public sealed record OpenForeclosurePreventionCaseCommand(
    string GroundLeaseId,
    string ServicerName,
    int DelinquentMonths,
    DateOnly RecordedDate,
    UserId RecordedBy);
