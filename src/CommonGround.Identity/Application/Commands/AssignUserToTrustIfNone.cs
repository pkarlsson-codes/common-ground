using CommonGround.Identity.Domain.Users;
using CommonGround.Identity.Domain.Users.Events;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.Identity.Application.Commands;

/// <summary>Command to idempotently assign a user to a trust if they don't already have one.</summary>
/// <param name="UserId">The ID of the user to assign.</param>
/// <param name="TrustId">The ID of the trust to assign the user to.</param>
public sealed record AssignUserToTrustIfNone(UserId UserId, TrustId TrustId);

/// <summary>Handles <see cref="AssignUserToTrustIfNone"/>.</summary>
public static class AssignUserToTrustIfNoneHandler
{
    /// <summary>Assigns the user to the trust, or does nothing when they already belong to one.</summary>
    public static UserTrustAssigned? Handle(AssignUserToTrustIfNone command, [WriteAggregate] User user)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(user);

        return user.AssignTrustIfNone(command.TrustId);
    }
}
