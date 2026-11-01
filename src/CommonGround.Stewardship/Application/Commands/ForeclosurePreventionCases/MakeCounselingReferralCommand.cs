using CommonGround.SharedKernel.Domain;
using CommonGround.Stewardship.Domain.ForeclosurePreventionCases;

namespace CommonGround.Stewardship.Application.Commands.ForeclosurePreventionCases;

/// <summary>
/// Command to make a counseling referral for a foreclosure prevention case.
/// </summary>
/// <param name="ForeclosurePreventionCaseId">The ID of the foreclosure prevention case.</param>
/// <param name="AgencyName">The name of the counseling agency.</param>
/// <param name="ReferralDate">The date of the referral.</param>
/// <param name="ReferredBy">The user ID of who made the referral.</param>
public sealed record MakeCounselingReferralCommand(
    ForeclosurePreventionCaseId ForeclosurePreventionCaseId,
    string AgencyName,
    DateOnly ReferralDate,
    UserId ReferredBy);
