namespace CommonGround.Stewardship.Domain.StewardshipCases;

/// <summary>The lifecycle status of a property inspection.</summary>
public enum PropertyInspectionStatus
{
    /// <summary>The inspection has been scheduled.</summary>
    Scheduled = 0,

    /// <summary>The inspection has been completed.</summary>
    Completed = 1,

    /// <summary>The inspection was cancelled.</summary>
    Cancelled = 2
}
