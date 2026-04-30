using CommonGround.PropertyRegistry.Domain.LeasableUnits;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Persistence;

namespace CommonGround.PropertyRegistry.Application.Commands;

/// <summary>Completes the resale closing for a home.</summary>
/// <param name="UnitId">The unit to transition.</param>
/// <param name="ChangedBy">The staff user making the change.</param>
public sealed record CompleteResaleClosing(LeasableUnitId UnitId, UserId ChangedBy);

/// <summary>Handles <see cref="CompleteResaleClosing"/>.</summary>
public static class CompleteResaleClosingHandler
{
    /// <summary>Loads the unit, completes resale closing, and persists.</summary>
    public static async Task Handle(
        CompleteResaleClosing command,
        ILeasableUnitRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var unit = await repository.GetByIdAsync(command.UnitId, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(command.UnitId.Value);

        ((Home)unit).CompleteResaleClosing(command.ChangedBy);

        repository.Stage(unit);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}
