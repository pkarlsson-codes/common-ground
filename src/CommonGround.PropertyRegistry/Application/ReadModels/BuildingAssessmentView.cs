namespace CommonGround.PropertyRegistry.Application.ReadModels;

/// <summary>Read model for a single building assessment event.</summary>
public class BuildingAssessmentView
{
    /// <summary>The Marten event ID for this assessment record.</summary>
    public Guid Id { get; set; }

    /// <summary>The ID of the building that was assessed.</summary>
    public string BuildingId { get; set; } = string.Empty;

    /// <summary>The assessed value of the building at the time of assessment.</summary>
    public decimal AssessedValue { get; set; }

    /// <summary>The date on which the assessment was performed.</summary>
    public DateOnly AssessmentDate { get; set; }

    /// <summary>Optional notes describing the condition of the building.</summary>
    public string? ConditionNotes { get; set; }

    /// <summary>The identifier of the person who performed the assessment.</summary>
    public string AssessedBy { get; set; } = string.Empty;

    /// <summary>The timestamp at which the assessment event was recorded.</summary>
    public DateTimeOffset RecordedAt { get; set; }
}
