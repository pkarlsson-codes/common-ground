namespace CommonGround.Stewardship.Domain.ForeclosurePreventionCases.ChildEntities;

/// <summary>A forbearance request submitted to a lender on behalf of the homeowner.</summary>
public sealed class ForbearanceRequest
{
    /// <summary>The unique identifier for this request.</summary>
    public Guid Id { get; internal set; }

    /// <summary>The name of the lender to whom the request was submitted.</summary>
    public string LenderName { get; internal set; } = string.Empty;

    /// <summary>The date the forbearance request was submitted.</summary>
    public DateOnly RequestedDate { get; internal set; }

    /// <summary>The lender's decision on the forbearance request.</summary>
    public ForbearanceOutcome? Outcome { get; internal set; }

    /// <summary>The date the lender's decision was recorded.</summary>
    public DateOnly? OutcomeDate { get; internal set; }

    /// <summary>Additional notes on the forbearance agreement or denial.</summary>
    public string? Notes { get; internal set; }
}
