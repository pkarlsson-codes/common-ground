using CommonGround.Identity.Domain.Users;
using CommonGround.Identity.Domain.Users.Events;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.Identity.Application.Commands;

/// <summary>Command to take a user out of a trust that is being deleted.</summary>
/// <param name="UserId">The member.</param>
/// <param name="TrustId">The trust being deleted.</param>
public sealed record LeaveDeletedTrust(UserId UserId, TrustId TrustId);

/// <summary>Handles <see cref="LeaveDeletedTrust"/>.</summary>
public static class LeaveDeletedTrustHandler
{
    /// <summary>Takes the user out of the trust, or does nothing when they no longer belong to it.</summary>
    public static UserTrustDeleted? Handle(LeaveDeletedTrust command, [WriteAggregate] User user)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(user);

        return user.LeaveDeletedTrust(command.TrustId);
    }
}
