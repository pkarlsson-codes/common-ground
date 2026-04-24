namespace CommonGround.Stewardship.Domain.ForeclosurePreventionCases;

/// <summary>The lifecycle status of a foreclosure prevention case.</summary>
public enum ForeclosurePreventionCaseStatus
{
    /// <summary>The case has been opened but no interventions have been recorded.</summary>
    Opened = 0,

    /// <summary>At least one intervention has been recorded.</summary>
    Active = 1,

    /// <summary>The case has been resolved.</summary>
    Resolved = 2
}
