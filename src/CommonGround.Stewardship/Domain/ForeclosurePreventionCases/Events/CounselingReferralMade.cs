using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.ForeclosurePreventionCases.Events;

/// <summary>Raised when a housing counseling referral is made.</summary>
/// <param name="AggregateId">The foreclosure prevention case identifier.</param>
/// <param name="ReferralId">The new referral identifier.</param>
/// <param name="AgencyName">The name of the counseling agency.</param>
/// <param name="ReferralDate">The date the referral was made.</param>
/// <param name="ReferredBy">The staff member who made the referral.</param>
public sealed record CounselingReferralMade(
    string AggregateId,
    Guid ReferralId,
    string AgencyName,
    DateOnly ReferralDate,
    string ReferredBy) : DomainEvent(AggregateId);
