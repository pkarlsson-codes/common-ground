using CommonGround.Identity.Application.Commands;
using CommonGround.SharedKernel.Domain;
using TrustOnboardingRequestedIntegration = CommonGround.TrustManagement.Contracts.Events.TrustOnboardingRequested;

namespace CommonGround.Identity.Application.EventHandlers;

/// <summary>Handles trust onboarding events from TrustManagement BC and assigns user to the trust.</summary>
public static class TrustOnboardingRequestedHandler
{
    /// <summary>Turns the integration event into the command that assigns the creator to the trust.</summary>
    public static AssignUserToTrustIfNone Handle(TrustOnboardingRequestedIntegration e)
    {
        ArgumentNullException.ThrowIfNull(e);

        return new AssignUserToTrustIfNone(UserId.Create(e.CreatorUserId), TrustId.Create(e.TrustId));
    }
}
