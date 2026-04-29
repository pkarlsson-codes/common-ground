using CommonGround.PropertyRegistry.Domain.LeasableUnits;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Persistence;

namespace CommonGround.PropertyRegistry.Application.Commands;

/// <summary>Abandons resale in favour of redevelopment for a dwelling unit.</summary>
/// <param name="UnitId">The unit to transition.</param>
/// <param name="ChangedBy">The staff user making the change.</param>
public sealed record AbandonResaleForRedevelopment(LeasableUnitId UnitId, UserId ChangedBy);

/// <summary>Handles <see cref="AbandonResaleForRedevelopment"/>.</summary>
public static class AbandonResaleForRedevelopmentHandler
{
    /// <summary>Loads the unit, abandons resale for redevelopment, and persists.</summary>
    public static async Task Handle(
        AbandonResaleForRedevelopment command,
        ILeasableUnitRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var unit = await repository.GetByIdAsync(command.UnitId, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(command.UnitId.Value);

        ((DwellingUnit)unit).AbandonResaleForRedevelopment(command.ChangedBy);

        repository.Stage(unit);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}
