namespace CommonGround.Stewardship.Domain.StewardshipCases.ChildEntities;

/// <summary>A scheduled property inspection for a stewardship case.</summary>
public sealed class PropertyInspection
{
    /// <summary>The unique identifier for this inspection.</summary>
    public Guid Id { get; internal set; }

    /// <summary>The date the inspection is scheduled to occur.</summary>
    public DateOnly ScheduledDate { get; internal set; }

    /// <summary>The name of the inspector performing the inspection.</summary>
    public string InspectorName { get; internal set; } = string.Empty;

    /// <summary>Whether occupancy was confirmed during the inspection.</summary>
    public bool? OccupancyConfirmed { get; internal set; }

    /// <summary>Notes on the physical condition of the property.</summary>
    public string? ConditionNotes { get; internal set; }

    /// <summary>The current status of this inspection.</summary>
    public PropertyInspectionStatus Status { get; internal set; }

    /// <summary>The date the inspection was completed.</summary>
    public DateOnly? CompletedDate { get; internal set; }
}
