using CommonGround.PropertyRegistry.Domain.LeasableUnits;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Persistence;

namespace CommonGround.PropertyRegistry.Application.Commands;

/// <summary>Initiates resale for a home.</summary>
/// <param name="UnitId">The unit to transition.</param>
/// <param name="ChangedBy">The staff user making the change.</param>
public sealed record InitiateResale(LeasableUnitId UnitId, UserId ChangedBy);

/// <summary>Handles <see cref="InitiateResale"/>.</summary>
public static class InitiateResaleHandler
{
    /// <summary>Loads the unit, initiates resale, and persists.</summary>
    public static async Task Handle(
        InitiateResale command,
        ILeasableUnitRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var unit = await repository.GetByIdAsync(command.UnitId, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(command.UnitId.Value);

        ((Home)unit).InitiateResale(command.ChangedBy);

        repository.Stage(unit);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}
