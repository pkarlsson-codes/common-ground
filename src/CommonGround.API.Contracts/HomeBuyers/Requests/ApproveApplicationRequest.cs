namespace CommonGround.API.Contracts.HomeBuyers.Requests;

/// <summary>Request to approve a submitted application, putting the household on the waiting list.</summary>
public sealed record ApproveApplicationRequest(
    DateOnly ApprovedDate,
    string? Notes);