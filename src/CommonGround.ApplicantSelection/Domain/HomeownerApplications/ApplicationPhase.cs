using CommonGround.SharedKernel.Domain;

namespace CommonGround.ApplicantSelection.Domain.HomeownerApplications;

/// <summary>High-level lifecycle phase of a homeowner application.</summary>
public sealed class ApplicationPhase : Enumeration<ApplicationPhase>
{
    /// <summary>Application submitted, eligibility not yet determined.</summary>
    public static readonly ApplicationPhase Submitted = new(0, nameof(Submitted));

    /// <summary>Applicant is eligible; working through counseling and prequalification.</summary>
    public static readonly ApplicationPhase Active = new(1, nameof(Active));

    /// <summary>All checklist items satisfied; applicant is in the waiting pool.</summary>
    public static readonly ApplicationPhase Waitlisted = new(2, nameof(Waitlisted));

    /// <summary>Applicant has been matched to a specific home.</summary>
    public static readonly ApplicationPhase Selected = new(3, nameof(Selected));

    /// <summary>Eligibility check failed.</summary>
    public static readonly ApplicationPhase EligibilityDenied = new(4, nameof(EligibilityDenied));

    /// <summary>Application denied by staff after eligibility.</summary>
    public static readonly ApplicationPhase Denied = new(5, nameof(Denied));

    /// <summary>Applicant withdrew.</summary>
    public static readonly ApplicationPhase Withdrawn = new(6, nameof(Withdrawn));

    /// <summary>Whether this phase allows no further transitions.</summary>
    public bool IsTerminal =>
        this == EligibilityDenied || this == Denied || this == Withdrawn || this == Selected;

    private ApplicationPhase(int value, string name) : base(value, name) { }
}