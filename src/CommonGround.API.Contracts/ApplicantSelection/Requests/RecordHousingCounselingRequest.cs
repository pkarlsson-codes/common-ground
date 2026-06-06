namespace CommonGround.API.Contracts.ApplicantSelection.Requests;

/// <summary>Request to record housing counseling completion.</summary>
public sealed record RecordHousingCounselingRequest(
    string CertificationNumber,
    string CompletedDate,
    string CounselorOrganization);