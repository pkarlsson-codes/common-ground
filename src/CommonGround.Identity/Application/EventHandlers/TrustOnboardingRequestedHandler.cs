using CommonGround.Identity.Application.Commands;
using Wolverine;
using TrustOnboardingRequestedIntegration = CommonGround.TrustManagement.Contracts.Events.TrustOnboardingRequested;

namespace CommonGround.Identity.Application.EventHandlers;

/// <summary>Handles trust onboarding events from TrustManagement BC and assigns user to the trust.</summary>
public sealed class TrustOnboardingRequestedHandler(IMessageBus bus)
{
    /// <summary>Receives integration event and dispatches command to assign user to trust.</summary>
    public async Task Handle(TrustOnboardingRequestedIntegration e, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(e);

        await bus.SendAsync(
            new AssignUserToTrustIfNone(e.CreatorUserId, e.TrustId))
            .ConfigureAwait(false);
    }
}
