namespace CommonGround.Stewardship.Domain.StewardshipCases;

/// <summary>The lifecycle status of a lease violation.</summary>
public enum LeaseViolationStatus
{
    /// <summary>The violation has been identified but no notice has been issued.</summary>
    Identified = 0,

    /// <summary>A formal notice has been issued to the homeowner.</summary>
    NoticeIssued = 1,

    /// <summary>The violation has been cured.</summary>
    Cured = 2,

    /// <summary>The violation has been escalated for further action.</summary>
    Escalated = 3
}
