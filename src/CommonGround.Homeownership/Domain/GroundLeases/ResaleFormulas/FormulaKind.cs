namespace CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;

/// <summary>Identifies which resale price formula applies to a ground lease.</summary>
public enum FormulaKind
{
    /// <summary>Resale price is based on a share of appraised appreciation.</summary>
    AppraisalBased = 0,

    /// <summary>Resale price is indexed to an external index (e.g., CPI).</summary>
    IndexBased = 1,

    /// <summary>Resale price grows at a fixed annual percentage.</summary>
    FixedPercentage = 2
}
