using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.TrustManagement.Domain.Trusts.Events;

/// <summary>Raised when a trust is created and onboarding should assign creator membership in Identity.</summary>
public sealed record TrustOnboardingRequested(
    string AggregateId,
    string TrustName,
    string CreatorUserId,
    string CorrelationId)
    : DomainEvent(AggregateId);
