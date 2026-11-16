namespace CommonGround.PropertyRegistry.Application.ReadModels;

/// <summary>Read model for a single home assessment, including the initial one recorded at registration.</summary>
public class HomeAssessmentView
{
    /// <summary>Gets or sets the Marten event ID of the assessment record.</summary>
    public Guid Id { get; set; }

    public string UnitId { get; set; } = string.Empty;

    public decimal AssessedValue { get; set; }

    public DateOnly AssessmentDate { get; set; }

    public string? ConditionNotes { get; set; }

    public string? AppraiserName { get; set; }

    public string RecordedBy { get; set; } = string.Empty;

    public DateTimeOffset RecordedAt { get; set; }
}
