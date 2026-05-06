using CommonGround.PropertyRegistry.Domain.Homes;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

namespace CommonGround.PropertyRegistry.Application.Commands;

/// <summary>Records a new assessed value for a home.</summary>
/// <param name="HomeId">The home being assessed.</param>
/// <param name="AssessedValue">The new assessed value in dollars.</param>
/// <param name="AssessmentDate">The date of the assessment.</param>
/// <param name="AppraiserName">The name of the appraiser who performed the assessment.</param>
/// <param name="ConditionNotes">Optional notes about the unit's condition.</param>
/// <param name="RecordedById">The staff user recording the assessment.</param>
public sealed record RecordHomeAssessment(
    HomeId HomeId,
    decimal AssessedValue,
    DateOnly AssessmentDate,
    string? AppraiserName,
    string? ConditionNotes,
    UserId RecordedById);

/// <summary>Handles <see cref="RecordHomeAssessment"/>.</summary>
public static class RecordHomeAssessmentHandler
{
    /// <summary>Loads the unit, records the assessment, and persists.</summary>
    public static async Task Handle(
        RecordHomeAssessment command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var stream = await session.Events.FetchForWriting<Home>(command.HomeId.Value, cancellationToken).ConfigureAwait(false);
        var home = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(Home), command.HomeId.Value);

        home.RecordAssessment(command.AssessedValue, command.AssessmentDate, command.AppraiserName, command.ConditionNotes, command.RecordedById);

        home.ValidateInvariants();
        stream.AppendMany(home.UncommittedEvents.ToArray());
        home.UncommittedEvents.MarkAsCommitted();
    }
}
