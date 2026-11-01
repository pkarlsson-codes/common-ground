using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using Wolverine.Marten;

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
    public static StewardshipCheckInRecorded Handle(RecordStewardshipCheckIn command, [WriteAggregate] GroundLease lease)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(lease);

        return lease.RecordStewardshipCheckIn(command.CheckInDate, command.Notes, command.RecordedBy);
    }
}
