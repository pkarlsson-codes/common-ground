namespace CommonGround.API.Contracts.HomeBuyers.Requests;

/// <summary>Request to deny an application.</summary>
public sealed record DenyApplicationRequest(
    string Reason,
    string DeniedDate);