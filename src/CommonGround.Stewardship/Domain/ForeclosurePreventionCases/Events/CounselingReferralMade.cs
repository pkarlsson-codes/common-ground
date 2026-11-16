using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.ForeclosurePreventionCases.Events;

public sealed record CounselingReferralMade(
    string AggregateId,
    Guid ReferralId,
    string AgencyName,
    DateOnly ReferralDate,
    string ReferredBy) : DomainEvent(AggregateId);
