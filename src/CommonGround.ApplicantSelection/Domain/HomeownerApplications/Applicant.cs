namespace CommonGround.ApplicantSelection.Domain.HomeownerApplications;

/// <summary>Contact information for a prospective homeowner.</summary>
public sealed record Applicant(string FullName, string Email, string? Phone);