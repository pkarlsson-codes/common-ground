using CommonGround.SharedKernel.Domain;
using CommonGround.TrustManagement.Domain.TrustInvitations;
using CommonGround.TrustManagement.Domain.TrustInvitations.Events;
using Wolverine.Marten;

namespace CommonGround.TrustManagement.Application.Commands;

/// <summary>Cancels an unused invitation so its link stops working.</summary>
/// <param name="TrustInvitationId">The invitation.</param>
/// <param name="TrustId">The member's trust.</param>
/// <param name="CancelledBy">The member cancelling.</param>
public sealed record CancelTrustInvitation(TrustInvitationId TrustInvitationId, TrustId TrustId, UserId CancelledBy);

/// <summary>Handles <see cref="CancelTrustInvitation"/>.</summary>
public static class CancelTrustInvitationHandler
{
    /// <summary>Cancels the invitation, or does nothing when it is already cancelled.</summary>
    public static TrustInvitationCancelled? Handle(CancelTrustInvitation command, [WriteAggregate] TrustInvitation invitation)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(invitation);

        return invitation.Cancel(command.TrustId, command.CancelledBy);
    }
}
