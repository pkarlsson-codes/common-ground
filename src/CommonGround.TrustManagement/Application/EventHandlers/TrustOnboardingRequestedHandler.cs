using CommonGround.TrustManagement.Domain.Trusts.Events;
using TrustOnboardingRequestedIntegration = CommonGround.TrustManagement.Contracts.Events.TrustOnboardingRequested;

namespace CommonGround.TrustManagement.Application.EventHandlers;

/// <summary>Publishes trust onboarding integration events for downstream Identity processing.</summary>
public static class TrustOnboardingRequestedHandler
{
    /// <summary>Maps the trust onboarding domain event to the integration event Wolverine publishes.</summary>
    public static TrustOnboardingRequestedIntegration Handle(TrustOnboardingRequested @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return new TrustOnboardingRequestedIntegration(
            @event.AggregateId,
            @event.TrustName,
            @event.CreatorUserId,
            @event.CorrelationId);
    }
}
