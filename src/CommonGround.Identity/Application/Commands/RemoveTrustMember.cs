using CommonGround.Identity.Domain.Users;
using CommonGround.Identity.Domain.Users.Events;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.Identity.Application.Commands;

/// <summary>Command to remove a user from the trust they belong to.</summary>
/// <param name="UserId">The member to remove.</param>
/// <param name="TrustId">The trust of the member who removes them.</param>
/// <param name="RemovedBy">The member who removes them.</param>
public sealed record RemoveTrustMember(UserId UserId, TrustId TrustId, UserId RemovedBy);

/// <summary>Handles <see cref="RemoveTrustMember"/>.</summary>
public static class RemoveTrustMemberHandler
{
    /// <summary>Removes the user from the trust.</summary>
    public static UserRemovedFromTrust Handle(RemoveTrustMember command, [WriteAggregate] User user)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(user);

        return user.RemoveFromTrust(command.TrustId, command.RemovedBy);
    }
}
