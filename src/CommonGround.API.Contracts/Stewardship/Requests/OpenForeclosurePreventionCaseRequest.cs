namespace CommonGround.API.Contracts.Stewardship.Requests;

/// <summary>Request to open a new foreclosure prevention case for servicer delinquency.</summary>
public sealed record OpenForeclosurePreventionCaseRequest(
    string GroundLeaseId,
    string ServicerName,
    int DelinquentMonths,
    DateOnly RecordedDate);
