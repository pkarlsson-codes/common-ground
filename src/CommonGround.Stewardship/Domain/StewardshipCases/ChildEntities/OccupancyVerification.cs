namespace CommonGround.Stewardship.Domain.StewardshipCases.ChildEntities;

/// <summary>Annual occupancy verification for a stewardship case.</summary>
public sealed class OccupancyVerification
{
    /// <summary>The unique identifier for this verification.</summary>
    public Guid Id { get; internal set; }

    /// <summary>The calendar year this verification covers.</summary>
    public int PeriodYear { get; internal set; }

    /// <summary>The deadline by which the homeowner must submit the survey.</summary>
    public DateOnly? SurveyDeadline { get; internal set; }

    /// <summary>Whether the homeowner confirmed primary residency.</summary>
    public bool? IsPrimaryResident { get; internal set; }

    /// <summary>Whether the homeowner disclosed any sublet arrangement.</summary>
    public bool? SubletDisclosed { get; internal set; }

    /// <summary>Notes on any major changes reported by the homeowner.</summary>
    public string? MajorChanges { get; internal set; }

    /// <summary>The current status of this verification.</summary>
    public OccupancyVerificationStatus Status { get; internal set; }

    /// <summary>The date the homeowner submitted the survey.</summary>
    public DateOnly? SubmittedDate { get; internal set; }

    /// <summary>The date a verification or failure decision was recorded.</summary>
    public DateOnly? DecisionDate { get; internal set; }
}
