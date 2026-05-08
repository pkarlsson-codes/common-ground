using CommonGround.TrustManagement.Domain.Trusts.Events;
using Wolverine;
using TrustOnboardingRequestedIntegration = CommonGround.TrustManagement.Contracts.Events.TrustOnboardingRequested;

namespace CommonGround.TrustManagement.Application.EventHandlers;

/// <summary>Publishes trust onboarding integration events for downstream Identity processing.</summary>
public sealed class TrustOnboardingRequestedHandler(IMessageBus bus)
{
    /// <summary>Maps trust onboarding domain event to integration event and publishes it.</summary>
    public async Task Handle(TrustOnboardingRequested @event, CancellationToken ct)
    {
        await bus.PublishAsync(new TrustOnboardingRequestedIntegration(
            @event.AggregateId,
            @event.TrustName,
            @event.CreatorUserId,
            @event.CorrelationId)).ConfigureAwait(false);
    }
}
