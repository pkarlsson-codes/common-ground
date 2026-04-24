using CommonGround.SharedKernel.Domain;

namespace CommonGround.Stewardship.Domain.StewardshipCases;

/// <summary>The lifecycle status of a stewardship case.</summary>
public sealed class StewardshipCaseStatus : Enumeration<StewardshipCaseStatus>
{
    /// <summary>The case is currently active and open.</summary>
    public static readonly StewardshipCaseStatus Active = new(0, "Active");

    /// <summary>The case has been closed.</summary>
    public static readonly StewardshipCaseStatus Closed = new(1, "Closed");

    private StewardshipCaseStatus(int value, string displayName) : base(value, displayName) { }
}
