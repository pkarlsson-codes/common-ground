using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
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

        var stream = await session.Events.FetchForWriting<GroundLease>(command.GroundLeaseId.Value, cancellationToken).ConfigureAwait(false);
        var lease = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(GroundLease), command.GroundLeaseId.Value);

        lease.ChangeHomeowner(command.NewHomeowner, command.ChangedBy);

        lease.ValidateInvariants();
        stream.AppendMany(lease.UncommittedEvents.ToArray());
        lease.UncommittedEvents.MarkAsCommitted();
    }
}
