namespace CommonGround.Stewardship.Infrastructure.Persistence;

/// <summary>Denormalized read model for a stewardship case.</summary>
public sealed class StewardshipCaseView
{
    /// <summary>Gets or sets the stewardship case ID.</summary>
    public string Id { get; set; } = string.Empty;
    /// <summary>Gets or sets the ground lease ID.</summary>
    public string GroundLeaseId { get; set; } = string.Empty;
    /// <summary>Gets or sets the current status of the case.</summary>
    public string Status { get; set; } = string.Empty;
    /// <summary>Gets or sets the date the case was opened.</summary>
    public DateOnly OpenedDate { get; set; }
    /// <summary>Gets or sets the date the case was closed, if applicable.</summary>
    public DateOnly? ClosedDate { get; set; }
    /// <summary>Gets or sets the list of occupancy verifications.</summary>
    public List<OccupancyVerificationRecord> OccupancyVerifications { get; set; } = [];
    /// <summary>Gets or sets the list of property inspections.</summary>
    public List<PropertyInspectionRecord> PropertyInspections { get; set; } = [];
    /// <summary>Gets or sets the list of lease violations.</summary>
    public List<LeaseViolationRecord> LeaseViolations { get; set; } = [];
}

/// <summary>Record of an occupancy verification within a stewardship case.</summary>
public sealed class OccupancyVerificationRecord
{
    /// <summary>Gets or sets the occupancy verification ID.</summary>
    public string Id { get; set; } = string.Empty;
    /// <summary>Gets or sets the year for the verification period.</summary>
    public int PeriodYear { get; set; }
    /// <summary>Gets or sets the survey deadline, if applicable.</summary>
    public DateOnly? SurveyDeadline { get; set; }
    /// <summary>Gets or sets the status of the verification.</summary>
    public string Status { get; set; } = string.Empty;
    /// <summary>Gets or sets whether the respondent is the primary resident.</summary>
    public bool? IsPrimaryResident { get; set; }
    /// <summary>Gets or sets whether any sublets were disclosed.</summary>
    public bool? SubletDisclosed { get; set; }
    /// <summary>Gets or sets notes about major changes to the property.</summary>
    public string? MajorChanges { get; set; }
    /// <summary>Gets or sets the date the verification was submitted.</summary>
    public DateOnly? SubmittedDate { get; set; }
    /// <summary>Gets or sets the date a decision was made on the verification.</summary>
    public DateOnly? DecisionDate { get; set; }
    /// <summary>Gets or sets the reason if the verification was rejected.</summary>
    public string? Reason { get; set; }
}

/// <summary>Record of a property inspection within a stewardship case.</summary>
public sealed class PropertyInspectionRecord
{
    /// <summary>Gets or sets the property inspection ID.</summary>
    public string Id { get; set; } = string.Empty;
    /// <summary>Gets or sets the date the inspection is scheduled.</summary>
    public DateOnly ScheduledDate { get; set; }
    /// <summary>Gets or sets the name of the inspector.</summary>
    public string InspectorName { get; set; } = string.Empty;
    /// <summary>Gets or sets the status of the inspection.</summary>
    public string Status { get; set; } = string.Empty;
    /// <summary>Gets or sets whether occupancy was confirmed.</summary>
    public bool? OccupancyConfirmed { get; set; }
    /// <summary>Gets or sets notes about the property condition.</summary>
    public string? ConditionNotes { get; set; }
    /// <summary>Gets or sets the date the inspection was completed.</summary>
    public DateOnly? CompletedDate { get; set; }
    /// <summary>Gets or sets the reason if the inspection was cancelled.</summary>
    public string? CancellationReason { get; set; }
}

/// <summary>Record of a lease violation within a stewardship case.</summary>
public sealed class LeaseViolationRecord
{
    /// <summary>Gets or sets the lease violation ID.</summary>
    public string Id { get; set; } = string.Empty;
    /// <summary>Gets or sets the type of lease violation.</summary>
    public string ViolationType { get; set; } = string.Empty;
    /// <summary>Gets or sets the description of the violation.</summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>Gets or sets the date the violation was identified.</summary>
    public DateOnly IdentifiedDate { get; set; }
    /// <summary>Gets or sets the status of the violation.</summary>
    public string Status { get; set; } = string.Empty;
    /// <summary>Gets or sets the date a notice was issued.</summary>
    public DateOnly? NoticeIssuedDate { get; set; }
    /// <summary>Gets or sets the deadline to cure the violation.</summary>
    public DateOnly? CureDeadline { get; set; }
    /// <summary>Gets or sets the date the violation was resolved.</summary>
    public DateOnly? ResolutionDate { get; set; }
    /// <summary>Gets or sets notes about the resolution.</summary>
    public string? ResolutionNotes { get; set; }
}
