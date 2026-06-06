namespace CommonGround.API.Contracts.ApplicantSelection.Requests;

/// <summary>Request to deny an application.</summary>
public sealed record DenyApplicationRequest(
    string Reason,
    string DeniedDate);