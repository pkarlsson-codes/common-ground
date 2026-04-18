using CommonGround.PropertyRegistry.Domain.Buildings;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Persistence;

namespace CommonGround.PropertyRegistry.Application.Commands;

/// <summary>Records a new assessed value for a building.</summary>
/// <param name="BuildingId">The building being assessed.</param>
/// <param name="AssessedValue">The new assessed value in dollars.</param>
/// <param name="AssessmentDate">The date of the assessment.</param>
/// <param name="ConditionNotes">Optional notes about the building's condition.</param>
/// <param name="AssessedBy">The staff user recording the assessment.</param>
public sealed record RecordBuildingAssessment(
    BuildingId BuildingId,
    decimal AssessedValue,
    DateOnly AssessmentDate,
    string? ConditionNotes,
    UserId AssessedBy);

/// <summary>Handles <see cref="RecordBuildingAssessment"/>.</summary>
public static class RecordBuildingAssessmentHandler
{
    /// <summary>Loads the building, records the assessment, and persists.</summary>
    public static async Task Handle(
        RecordBuildingAssessment command,
        IBuildingRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var building = await repository.GetByIdAsync(command.BuildingId, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(command.BuildingId.Value);

        building.RecordAssessment(command.AssessedValue, command.AssessmentDate, command.ConditionNotes, command.AssessedBy);

        repository.Stage(building);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}