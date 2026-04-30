using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;

namespace CommonGround.PropertyRegistry.Domain.Homes;

/// <summary>An assessed value snapshot recorded for a home at a point in time.</summary>
public sealed class UnitAssessment : ValueObject
{
    private UnitAssessment(decimal assessedValue, DateOnly assessmentDate, string? appraiserName, string? conditionNotes, UserId recordedById, DateTimeOffset occurredAt)
    {
        AssessedValue = assessedValue;
        AssessedAt = assessmentDate;
        AppraiserName = appraiserName;
        ConditionNotes = conditionNotes;
        RecordedById = recordedById;
        RecordedAt = occurredAt;
    }

    /// <summary>Creates and validates a new <see cref="UnitAssessment"/>.</summary>
    public static UnitAssessment Create(decimal assessedValue, DateOnly assessmentDate, string? appraiserName, string? conditionNotes, UserId recordedById, DateTimeOffset occurredAt)
    {
        ArgumentNullException.ThrowIfNull(recordedById);

        if (assessedValue < 0)
            throw new BusinessRuleViolationException("Assessed value cannot be negative.");

        if (assessmentDate > DateOnly.FromDateTime(DateTimeOffset.UtcNow.DateTime))
            throw new BusinessRuleViolationException("Assessment date cannot be in the future.");

        return new UnitAssessment(assessedValue, assessmentDate, appraiserName, conditionNotes, recordedById, occurredAt);
    }

    /// <summary>Reconstitutes a <see cref="UnitAssessment"/> from a stored event without revalidating.</summary>
    internal static UnitAssessment Reconstitute(decimal assessedValue, DateOnly assessmentDate, string? appraiserName, string? conditionNotes, UserId recordedById, DateTimeOffset occurredAt)
        => new(assessedValue, assessmentDate, appraiserName, conditionNotes, recordedById, occurredAt);

    /// <summary>Gets the assessed value of the home at the time of assessment.</summary>
    public decimal AssessedValue { get; }

    /// <summary>Gets the date when the assessment was performed.</summary>
    public DateOnly AssessedAt { get; }

    /// <summary>Gets the name of the appraiser who performed the assessment.</summary>
    public string? AppraiserName { get; }

    /// <summary>Gets any notes about the condition of the home at the time of assessment.</summary>
    public string? ConditionNotes { get; }

    /// <summary>Gets the ID of the staff user who recorded this assessment.</summary>
    public UserId RecordedById { get; }

    /// <summary>Gets the date and time when this assessment record was created.</summary>
    public DateTimeOffset RecordedAt { get; }

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return AssessedValue;
        yield return AssessedAt;
        yield return AppraiserName ?? string.Empty;
        yield return ConditionNotes ?? string.Empty;
        yield return RecordedAt;
        yield return RecordedById;
    }
}
