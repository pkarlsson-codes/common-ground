namespace CommonGround.API.Contracts.HomeBuyers.Requests;

/// <summary>Request to withdraw an application.</summary>
public sealed record WithdrawApplicationRequest(
    DateOnly WithdrawnDate,
    string? Reason);