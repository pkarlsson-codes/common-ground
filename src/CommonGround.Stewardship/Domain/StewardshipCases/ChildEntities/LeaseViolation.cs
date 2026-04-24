namespace CommonGround.Stewardship.Domain.StewardshipCases.ChildEntities;

/// <summary>A lease violation identified against a stewardship case.</summary>
public sealed class LeaseViolation
{
    /// <summary>The unique identifier for this violation.</summary>
    public Guid Id { get; internal set; }

    /// <summary>The category of the violation.</summary>
    public LeaseViolationType Type { get; internal set; }

    /// <summary>Description of the violation.</summary>
    public string Description { get; internal set; } = string.Empty;

    /// <summary>The date the violation was identified.</summary>
    public DateOnly IdentifiedDate { get; internal set; }

    /// <summary>The date a formal notice was issued to the homeowner.</summary>
    public DateOnly? NoticeIssuedDate { get; internal set; }

    /// <summary>The deadline by which the homeowner must cure the violation.</summary>
    public DateOnly? CureDeadline { get; internal set; }

    /// <summary>The current status of this violation.</summary>
    public LeaseViolationStatus Status { get; internal set; }

    /// <summary>The date the violation was resolved (cured or escalated).</summary>
    public DateOnly? ResolutionDate { get; internal set; }

    /// <summary>Notes on the resolution of the violation.</summary>
    public string? ResolutionNotes { get; internal set; }
}
