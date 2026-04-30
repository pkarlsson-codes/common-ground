using CommonGround.PropertyRegistry.Domain.LeasableUnits;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Persistence;

namespace CommonGround.PropertyRegistry.Application.Commands;

/// <summary>Executes a ground lease on a home.</summary>
/// <param name="UnitId">The unit to transition.</param>
/// <param name="ChangedBy">The staff user making the change.</param>
public sealed record ExecuteGroundLease(LeasableUnitId UnitId, UserId ChangedBy);

/// <summary>Handles <see cref="ExecuteGroundLease"/>.</summary>
public static class ExecuteGroundLeaseHandler
{
    /// <summary>Loads the unit, executes the ground lease, and persists.</summary>
    public static async Task Handle(
        ExecuteGroundLease command,
        ILeasableUnitRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var unit = await repository.GetByIdAsync(command.UnitId, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(command.UnitId.Value);

        ((Home)unit).ExecuteGroundLease(command.ChangedBy);

        repository.Stage(unit);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}
