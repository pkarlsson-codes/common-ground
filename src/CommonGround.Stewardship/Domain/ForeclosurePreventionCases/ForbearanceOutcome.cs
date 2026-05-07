using CommonGround.SharedKernel.Domain;

namespace CommonGround.Stewardship.Domain.ForeclosurePreventionCases;

/// <summary>The outcome of a forbearance request submitted to a lender.</summary>
public sealed class ForbearanceOutcome : Enumeration<ForbearanceOutcome>
{
    /// <summary>The lender granted the forbearance request.</summary>
    public static readonly ForbearanceOutcome Granted = new(0, "Granted");

    /// <summary>The lender denied the forbearance request.</summary>
    public static readonly ForbearanceOutcome Denied = new(1, "Denied");

    private ForbearanceOutcome(int value, string displayName) : base(value, displayName) { }
}
