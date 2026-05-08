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

        // Idempotent: if user already has a trust, consider it a success
        if (user.TrustId is not null)
        {
            return new AssignUserToTrustIfNoneResult(true);
        }

        // Assign the trust to the user
        user.AssignTrust(SharedKernel.Domain.TrustId.Create(command.TrustId));
        user.ValidateInvariants();

        // Persist the changes
        stream.AppendMany(user.UncommittedEvents.ToArray());
        user.UncommittedEvents.MarkAsCommitted();

        // Update the UserSubjectIndex to reflect the new TrustId
        // We need to load the index by UserId → find by looking it up from the user's Keycloak subject
        // For now, we'll update indexes for all documents matching this UserId
        var indexes = await session.Query<UserSubjectIndex>()
            .Where(x => x.UserId == command.UserId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var index in indexes)
        {
            index.TrustId = command.TrustId;
            session.Update(index);
        }

        return new AssignUserToTrustIfNoneResult(true);
    }
}
