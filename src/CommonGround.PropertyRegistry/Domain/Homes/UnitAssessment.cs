using CommonGround.SharedKernel.Domain;

namespace CommonGround.PropertyRegistry.Domain.Homes;

/// <summary>An assessed value snapshot recorded for a home at a point in time.</summary>
public sealed class UnitAssessment : ValueObject
{
    /// <summary>Initializes a new instance of <see cref="UnitAssessment"/>.</summary>
    public UnitAssessment(decimal assessedValue, DateOnly assessmentDate, string? conditionNotes, UserId assessedBy, DateTimeOffset occurredAt)
    {
        ArgumentNullException.ThrowIfNull(assessedBy);

        AssessedValue = assessedValue;
        AssessedAt = assessmentDate;
        ConditionNotes = conditionNotes;
        AssessedBy = assessedBy;
        RecordedAt = occurredAt;
    }

    /// <summary>Gets the assessed value of the home at the time of assessment.</summary>
    public decimal AssessedValue { get; }

    /// <summary>Gets the date when the assessment was performed.</summary>
    public DateOnly AssessedAt { get; }

    /// <summary>Gets any notes about the condition of the home at the time of assessment.</summary>
    public string? ConditionNotes { get; }

    /// <summary>Gets the user who performed the assessment.</summary>
    public UserId AssessedBy { get; }

    /// <summary>Gets the date and time when this assessment record was created.</summary>
    public DateTimeOffset RecordedAt { get; }

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return AssessedValue;
        yield return AssessedAt;
        yield return ConditionNotes ?? string.Empty;
        yield return RecordedAt;
        yield return AssessedBy;
    }
}
