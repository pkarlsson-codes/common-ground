using CommonGround.TrustManagement.Contracts.Events;
using CommonGround.TrustManagement.Domain.TrustInvitations.Events;

namespace CommonGround.TrustManagement.Application.EventHandlers;

/// <summary>Tells Identity that a user joined a trust with an invitation.</summary>
public static class TrustInvitationAcceptedHandler
{
    /// <summary>Maps the acceptance to the integration event Wolverine publishes.</summary>
    public static TrustMemberJoined Handle(TrustInvitationAccepted @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return new TrustMemberJoined(@event.TrustId, @event.UserId);
    }
}
