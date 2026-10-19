namespace CommonGround.API.Contracts.HomeBuyers.Requests;

/// <summary>Request to approve eligibility.</summary>
public sealed record ApproveApplicationRequest(
    string ApprovedDate,
    string? Notes);