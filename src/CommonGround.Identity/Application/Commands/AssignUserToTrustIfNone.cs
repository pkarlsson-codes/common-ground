using CommonGround.Identity.Domain.Users;
using CommonGround.Identity.Infrastructure.Persistence;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

namespace CommonGround.Identity.Application.Commands;

/// <summary>Command to idempotently assign a user to a trust if they don't already have one.</summary>
/// <param name="UserId">The ID of the user to assign.</param>
/// <param name="TrustId">The ID of the trust to assign the user to.</param>
public sealed record AssignUserToTrustIfNone(string UserId, string TrustId);

/// <summary>Result of the trust assignment operation.</summary>
/// <param name="Success">Whether the assignment succeeded.</param>
public sealed record AssignUserToTrustIfNoneResult(bool Success);

/// <summary>Handles <see cref="AssignUserToTrustIfNone"/>.</summary>
public static class AssignUserToTrustIfNoneHandler
{
    /// <summary>Idempotently assigns the user to a trust if they don't already have one.</summary>
    public static async Task<AssignUserToTrustIfNoneResult> Handle(
        AssignUserToTrustIfNone command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var stream = await session.Events.FetchForWriting<User>(command.UserId, cancellationToken).ConfigureAwait(false);
        var user = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(User), command.UserId);

        if (user.TrustId is not null)
        {
            return new AssignUserToTrustIfNoneResult(true);
        }

        user.AssignTrust(SharedKernel.Domain.TrustId.Create(command.TrustId));
        user.ValidateInvariants();

        stream.AppendMany(user.UncommittedEvents.ToArray());
        user.UncommittedEvents.MarkAsCommitted();

        return new AssignUserToTrustIfNoneResult(true);
    }
}
