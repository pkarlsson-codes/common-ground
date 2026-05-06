using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Records a stewardship check-in visit on a ground lease.</summary>
public sealed record RecordStewardshipCheckIn(
    GroundLeaseId GroundLeaseId,
    DateOnly CheckInDate,
    string Notes,
    UserId RecordedBy);

/// <summary>Handles <see cref="RecordStewardshipCheckIn"/>.</summary>
public static class RecordStewardshipCheckInHandler
{
    /// <summary>Loads the lease, records the stewardship check-in, and persists.</summary>
    public static async Task Handle(
        RecordStewardshipCheckIn command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var stream = await session.Events.FetchForWriting<GroundLease>(command.GroundLeaseId.Value, cancellationToken).ConfigureAwait(false);
        var lease = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(GroundLease), command.GroundLeaseId.Value);

        lease.RecordStewardshipCheckIn(command.CheckInDate, command.Notes, command.RecordedBy);

        lease.ValidateInvariants();
        stream.AppendMany(lease.UncommittedEvents.ToArray());
        lease.UncommittedEvents.MarkAsCommitted();
    }
}
