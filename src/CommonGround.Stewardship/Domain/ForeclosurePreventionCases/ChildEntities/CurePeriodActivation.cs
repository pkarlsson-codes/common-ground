namespace CommonGround.Stewardship.Domain.ForeclosurePreventionCases.ChildEntities;

/// <summary>An activation of the CLT cure period under a lender recognition agreement.</summary>
public sealed class CurePeriodActivation
{
    /// <summary>The unique identifier for this activation.</summary>
    public Guid Id { get; internal set; }

    /// <summary>Reference to the lender recognition agreement (opaque string).</summary>
    public string LraId { get; internal set; } = string.Empty;

    /// <summary>The date the cure period was activated.</summary>
    public DateOnly ActivationDate { get; internal set; }

    /// <summary>The number of days in the cure period.</summary>
    public int CurePeriodDays { get; internal set; }

    /// <summary>The deadline by which the delinquency must be cured.</summary>
    public DateOnly CureDeadline { get; internal set; }

    /// <summary>The amount cured, if the cure was successful.</summary>
    public decimal? AmountCured { get; internal set; }

    /// <summary>Whether the cure was successful.</summary>
    public bool? CureSuccessful { get; internal set; }
}
