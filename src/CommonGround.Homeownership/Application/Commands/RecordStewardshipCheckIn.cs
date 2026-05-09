using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Extensions;
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

        var lease = await session
            .LoadAggregateAsync<GroundLease>(command.GroundLeaseId, cancellationToken)
            .ConfigureAwait(false);

        lease.RecordStewardshipCheckIn(command.CheckInDate, command.Notes, command.RecordedBy);

        session.UpdateAggregate(lease);
    }
}
