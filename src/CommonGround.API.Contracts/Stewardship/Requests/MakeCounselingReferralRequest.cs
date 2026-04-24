namespace CommonGround.API.Contracts.Stewardship.Requests;

/// <summary>Request to make a housing counseling referral on a foreclosure prevention case.</summary>
public sealed record MakeCounselingReferralRequest(
    string AgencyName,
    DateOnly ReferralDate);
