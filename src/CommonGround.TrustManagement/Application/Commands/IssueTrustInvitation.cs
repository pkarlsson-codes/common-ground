using CommonGround.SharedKernel.Domain;
using CommonGround.TrustManagement.Domain.TrustInvitations;
using Wolverine.Marten;

namespace CommonGround.TrustManagement.Application.Commands;

/// <summary>Makes an invite link to the trust for a colleague.</summary>
/// <param name="TrustInvitationId">The new invitation's id, chosen by the caller.</param>
/// <param name="TrustId">The member's trust.</param>
/// <param name="Email">The colleague's address, as entered.</param>
/// <param name="TokenHash">The hash of the link's token; the caller keeps the token to show once.</param>
/// <param name="IssuedBy">The member making the invitation.</param>
/// <param name="IssuedAt">The current instant.</param>
public sealed record IssueTrustInvitation(
    TrustInvitationId TrustInvitationId,
    TrustId TrustId,
    string Email,
    string TokenHash,
    UserId IssuedBy,
    DateTimeOffset IssuedAt);

/// <summary>Handles <see cref="IssueTrustInvitation"/>.</summary>
public static class IssueTrustInvitationHandler
{
    /// <summary>Starts the invitation's stream.</summary>
    public static IStartStream Handle(IssueTrustInvitation command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var issued = TrustInvitation.Issue(
            command.TrustInvitationId,
            command.TrustId,
            EmailAddress.Create(command.Email),
            command.TokenHash,
            command.IssuedBy,
            command.IssuedAt);

        return MartenOps.StartStream<TrustInvitation>(command.TrustInvitationId.Value, issued);
    }
}
