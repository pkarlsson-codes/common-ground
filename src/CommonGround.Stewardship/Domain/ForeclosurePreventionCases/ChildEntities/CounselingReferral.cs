namespace CommonGround.Stewardship.Domain.ForeclosurePreventionCases.ChildEntities;

/// <summary>A referral to a housing counseling agency.</summary>
public sealed class CounselingReferral
{
    /// <summary>The unique identifier for this referral.</summary>
    public Guid Id { get; internal set; }

    /// <summary>The name of the counseling agency.</summary>
    public string AgencyName { get; internal set; } = string.Empty;

    /// <summary>The date the referral was made.</summary>
    public DateOnly ReferralDate { get; internal set; }

    /// <summary>The outcome of the counseling session.</summary>
    public string? Outcome { get; internal set; }

    /// <summary>The date the counseling outcome was recorded.</summary>
    public DateOnly? OutcomeDate { get; internal set; }
}
