namespace CommonGround.PropertyRegistry.Application.ReadModels;

/// <summary>Read model for a single dwelling unit assessment event.</summary>
public class DwellingUnitAssessmentView
{
    /// <summary>The Marten event ID for this assessment record.</summary>
    public Guid Id { get; set; }

    /// <summary>The ID of the unit that was assessed.</summary>
    public string UnitId { get; set; } = string.Empty;

    /// <summary>The assessed value of the unit at the time of assessment.</summary>
    public decimal AssessedValue { get; set; }

    /// <summary>The date on which the assessment was performed.</summary>
    public DateOnly AssessmentDate { get; set; }

    /// <summary>Optional notes describing the condition of the unit.</summary>
    public string? ConditionNotes { get; set; }

    /// <summary>The identifier of the person who performed the assessment.</summary>
    public string AssessedBy { get; set; } = string.Empty;

    /// <summary>The timestamp at which the assessment event was recorded.</summary>
    public DateTimeOffset RecordedAt { get; set; }
}
