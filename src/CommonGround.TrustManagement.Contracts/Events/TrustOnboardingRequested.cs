using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.TrustManagement.Contracts.Events;

/// <summary>Integration event published when trust onboarding should assign membership for the creator.</summary>
public sealed record TrustOnboardingRequested(
    string TrustId,
    string TrustName,
    string CreatorUserId,
    string CorrelationId) : IntegrationEvent;
