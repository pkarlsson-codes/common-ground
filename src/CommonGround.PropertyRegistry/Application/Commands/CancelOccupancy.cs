using CommonGround.PropertyRegistry.Domain.LeasableUnits;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Persistence;

namespace CommonGround.PropertyRegistry.Application.Commands;

/// <summary>Cancels pending occupancy for a dwelling unit.</summary>
/// <param name="UnitId">The unit to transition.</param>
/// <param name="ChangedBy">The staff user making the change.</param>
public sealed record CancelOccupancy(LeasableUnitId UnitId, UserId ChangedBy);

/// <summary>Handles <see cref="CancelOccupancy"/>.</summary>
public static class CancelOccupancyHandler
{
    /// <summary>Loads the unit, cancels occupancy, and persists.</summary>
    public static async Task Handle(
        CancelOccupancy command,
        ILeasableUnitRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var unit = await repository.GetByIdAsync(command.UnitId, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(command.UnitId.Value);

        ((DwellingUnit)unit).CancelOccupancy(command.ChangedBy);

        repository.Stage(unit);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}
