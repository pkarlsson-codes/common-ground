using CommonGround.Homeownership.Domain.GroundLeases.Events;
using Wolverine;
using HomeownerOnboardedIntegration = CommonGround.Homeownership.Contracts.Events.HomeownerOnboarded;

namespace CommonGround.Homeownership.Application.EventHandlers;

/// <summary>Publishes an integration event when a homeowner is onboarded, so other BCs can react.</summary>
public sealed class HomeownerOnboardedHandler(IMessageBus bus)
{
    /// <summary>Handles the domain event and publishes the integration event to the bus.</summary>
    public async Task Handle(HomeownerOnboarded @event, CancellationToken ct)
    {
        await bus.PublishAsync(new HomeownerOnboardedIntegration(
            @event.AggregateId,
            @event.FullName,
            @event.Email,
            @event.Phone,
            @event.OnboardedBy)).ConfigureAwait(false);
    }
}
