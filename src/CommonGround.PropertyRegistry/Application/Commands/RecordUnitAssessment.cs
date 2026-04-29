using CommonGround.PropertyRegistry.Domain.LeasableUnits;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Persistence;

namespace CommonGround.PropertyRegistry.Application.Commands;

/// <summary>Records a new assessed value for a leasable unit.</summary>
/// <param name="UnitId">The unit being assessed.</param>
/// <param name="AssessedValue">The new assessed value in dollars.</param>
/// <param name="AssessmentDate">The date of the assessment.</param>
/// <param name="ConditionNotes">Optional notes about the unit's condition.</param>
/// <param name="AssessedBy">The staff user recording the assessment.</param>
public sealed record RecordUnitAssessment(
    LeasableUnitId UnitId,
    decimal AssessedValue,
    DateOnly AssessmentDate,
    string? ConditionNotes,
    UserId AssessedBy);

/// <summary>Handles <see cref="RecordUnitAssessment"/>.</summary>
public static class RecordUnitAssessmentHandler
{
    /// <summary>Loads the unit, records the assessment, and persists.</summary>
    public static async Task Handle(
        RecordUnitAssessment command,
        ILeasableUnitRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var unit = await repository.GetByIdAsync(command.UnitId, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(command.UnitId.Value);

        unit.RecordAssessment(command.AssessedValue, command.AssessmentDate, command.ConditionNotes, command.AssessedBy);

        repository.Stage(unit);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}
