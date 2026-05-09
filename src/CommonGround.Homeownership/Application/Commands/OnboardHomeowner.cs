using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Onboards the first homeowner to a ground lease.</summary>
public sealed record OnboardHomeowner(
    GroundLeaseId GroundLeaseId,
    Homeowner Homeowner,
    UserId OnboardedBy);

/// <summary>Handles <see cref="OnboardHomeowner"/>.</summary>
public static class OnboardHomeownerHandler
{
    /// <summary>Loads the lease, onboards the homeowner, and persists.</summary>
    public static async Task Handle(
        OnboardHomeowner command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var lease = await session
            .LoadAggregateAsync<GroundLease>(command.GroundLeaseId, cancellationToken)
            .ConfigureAwait(false);

        lease.OnboardHomeowner(command.Homeowner, command.OnboardedBy);

        session.UpdateAggregate(lease);
    }
}
