using CommonGround.SharedKernel.Domain;

namespace CommonGround.Stewardship.Domain.ForeclosurePreventionCases;

/// <summary>The lifecycle status of a foreclosure prevention case.</summary>
public sealed class ForeclosurePreventionCaseStatus : Enumeration<ForeclosurePreventionCaseStatus>
{
    /// <summary>The case has been opened but no interventions have been recorded.</summary>
    public static readonly ForeclosurePreventionCaseStatus Opened = new(0, "Opened");

    /// <summary>At least one intervention has been recorded.</summary>
    public static readonly ForeclosurePreventionCaseStatus Active = new(1, "Active");

    /// <summary>The case has been resolved.</summary>
    public static readonly ForeclosurePreventionCaseStatus Resolved = new(2, "Resolved");

    private ForeclosurePreventionCaseStatus(int value, string displayName) : base(value, displayName) { }
}
