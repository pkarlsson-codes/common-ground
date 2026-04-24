namespace CommonGround.Stewardship.Domain.ForeclosurePreventionCases;

/// <summary>The outcome of a forbearance request submitted to a lender.</summary>
public enum ForbearanceOutcome
{
    /// <summary>The lender granted the forbearance request.</summary>
    Granted = 0,

    /// <summary>The lender denied the forbearance request.</summary>
    Denied = 1
}
