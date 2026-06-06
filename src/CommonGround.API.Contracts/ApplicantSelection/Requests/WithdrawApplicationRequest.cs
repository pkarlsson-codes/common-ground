namespace CommonGround.API.Contracts.ApplicantSelection.Requests;

/// <summary>Request to withdraw an application.</summary>
public sealed record WithdrawApplicationRequest(
    string WithdrawnDate,
    string? Reason);