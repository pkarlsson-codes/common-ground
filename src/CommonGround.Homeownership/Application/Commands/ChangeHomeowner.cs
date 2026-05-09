using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Changes the homeowner on a ground lease.</summary>
public sealed record ChangeHomeowner(
    GroundLeaseId GroundLeaseId,
    Homeowner NewHomeowner,
    UserId ChangedBy);

/// <summary>Handles <see cref="ChangeHomeowner"/>.</summary>
public static class ChangeHomeownerHandler
{
    /// <summary>Loads the lease, changes the homeowner, and persists.</summary>
    public static async Task Handle(
        ChangeHomeowner command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var lease = await session
            .LoadAggregateAsync<GroundLease>(command.GroundLeaseId, cancellationToken)
            .ConfigureAwait(false);

        lease.ChangeHomeowner(command.NewHomeowner, command.ChangedBy);

        session.UpdateAggregate(lease);
    }
}
