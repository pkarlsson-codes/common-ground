namespace CommonGround.API.Contracts.Stewardship;

/// <summary>
/// Published by the Lease Finance BC when lease fee arrears reach an escalation threshold.
/// Consumed by Stewardship to open a ForeclosurePreventionCase.
/// </summary>
/// <remarks>
/// Stub contract — Lease Finance BC is future work.
/// Fields are provisional; expand when Lease Finance BC is designed.
/// </remarks>
public sealed record LeaseFeeArrearsEscalated(
    string GroundLeaseId,
    int MonthsInArrears,
    decimal TotalAmountOwed,
    DateOnly EscalationDate);
