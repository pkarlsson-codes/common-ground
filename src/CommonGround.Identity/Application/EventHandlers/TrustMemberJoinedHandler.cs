using CommonGround.Identity.Application.Commands;
using CommonGround.SharedKernel.Domain;
using CommonGround.TrustManagement.Contracts.Events;

namespace CommonGround.Identity.Application.EventHandlers;

/// <summary>Assigns a user who joined a trust with an invitation to that trust.</summary>
public static class TrustMemberJoinedHandler
{
    /// <summary>Turns the integration event into the command that assigns the user to the trust.</summary>
    public static AssignUserToTrustIfNone Handle(TrustMemberJoined e)
    {
        ArgumentNullException.ThrowIfNull(e);

        return new AssignUserToTrustIfNone(UserId.Create(e.UserId), TrustId.Create(e.TrustId));
    }
}
