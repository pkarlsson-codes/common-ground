namespace CommonGround.API.Contracts.Stewardship.Requests;

/// <summary>Request to submit a mortgage forbearance request to a lender.</summary>
public sealed record RequestForbearanceRequest(
    string LenderName,
    DateOnly RequestedDate);
