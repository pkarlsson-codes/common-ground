using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Persistence;

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
        IGroundLeaseRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var lease = await repository.GetByIdAsync(command.GroundLeaseId, cancellationToken).ConfigureAwait(false)
            ?? throw new CommonGround.SharedKernel.Domain.Exceptions.AggregateNotFoundException(command.GroundLeaseId.Value);
        lease.RecordStewardshipCheckIn(command.CheckInDate, command.Notes, command.RecordedBy);

        repository.Stage(lease);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}
