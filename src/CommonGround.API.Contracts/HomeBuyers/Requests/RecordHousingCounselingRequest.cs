namespace CommonGround.API.Contracts.HomeBuyers.Requests;

/// <summary>Request to record housing counseling completion.</summary>
public sealed record RecordHousingCounselingRequest(
    string CertificationNumber,
    DateOnly CompletedDate,
    string CounselorOrganization);