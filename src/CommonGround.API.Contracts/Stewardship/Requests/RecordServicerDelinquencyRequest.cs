namespace CommonGround.API.Contracts.Stewardship.Requests;

/// <summary>Request to record a servicer delinquency report on a foreclosure prevention case.</summary>
public sealed record RecordServicerDelinquencyRequest(
    string ServicerName,
    int DelinquentMonths,
    DateOnly RecordedDate);
