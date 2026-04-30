using CommonGround.PropertyRegistry.Domain.Homes;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Persistence;

namespace CommonGround.PropertyRegistry.Application.Commands;

/// <summary>Records a new assessed value for a home.</summary>
/// <param name="HomeId">The home being assessed.</param>
/// <param name="AssessedValue">The new assessed value in dollars.</param>
/// <param name="AssessmentDate">The date of the assessment.</param>
/// <param name="ConditionNotes">Optional notes about the unit's condition.</param>
/// <param name="AssessedBy">The staff user recording the assessment.</param>
public sealed record RecordHomeAssessment(
    HomeId HomeId,
    decimal AssessedValue,
    DateOnly AssessmentDate,
    string? ConditionNotes,
    UserId AssessedBy);

/// <summary>Handles <see cref="RecordHomeAssessment"/>.</summary>
public static class RecordHomeAssessmentHandler
{
    /// <summary>Loads the unit, records the assessment, and persists.</summary>
    public static async Task Handle(
        RecordHomeAssessment command,
        IHomeRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var home = await repository.GetByIdAsync(command.HomeId, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(command.HomeId.Value);

        home.RecordAssessment(command.AssessedValue, command.AssessmentDate, command.ConditionNotes, command.AssessedBy);

        repository.Stage(home);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}
