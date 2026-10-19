using CommonGround.SharedKernel.Domain;

namespace CommonGround.HomeBuyers.Domain.HomeownerApplications;

/// <summary>High-level lifecycle phase of a homeowner application.</summary>
public sealed class ApplicationPhase : Enumeration<ApplicationPhase>
{
    /// <summary>Application submitted and awaiting a decision.</summary>
    public static readonly ApplicationPhase Submitted = new(0, nameof(Submitted));
    /// <summary>Application approved; the applicant can be selected for a home.</summary>
    public static readonly ApplicationPhase Approved = new(1, nameof(Approved));
    /// <summary>Application denied.</summary>
    public static readonly ApplicationPhase Denied = new(2, nameof(Denied));
    /// <summary>Application withdrawn by the applicant.</summary>
    public static readonly ApplicationPhase Withdrawn = new(3, nameof(Withdrawn));
    /// <summary>Applicant has been matched to a specific home.</summary>
    public static readonly ApplicationPhase Selected = new(4, nameof(Selected));
    /// <summary>Whether this phase allows no further transitions.</summary>
    public bool IsTerminal =>
        this == Denied || this == Withdrawn || this == Selected;

    private ApplicationPhase(int value, string name) : base(value, name) { }
}