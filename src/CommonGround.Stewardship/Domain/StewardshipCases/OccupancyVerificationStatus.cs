namespace CommonGround.Stewardship.Domain.StewardshipCases;

/// <summary>The lifecycle status of an occupancy verification.</summary>
public enum OccupancyVerificationStatus
{
    /// <summary>Awaiting homeowner submission.</summary>
    Pending = 0,

    /// <summary>Homeowner has submitted the survey.</summary>
    Submitted = 1,

    /// <summary>Verification confirmed the homeowner is the primary resident.</summary>
    Verified = 2,

    /// <summary>Verification failed.</summary>
    Failed = 3,

    /// <summary>The submission deadline has passed without a response.</summary>
    Overdue = 4
}
