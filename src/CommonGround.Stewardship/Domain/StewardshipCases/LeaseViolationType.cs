namespace CommonGround.Stewardship.Domain.StewardshipCases;

/// <summary>The category of a lease violation.</summary>
public enum LeaseViolationType
{
    /// <summary>Violation related to primary occupancy requirements.</summary>
    Occupancy = 0,

    /// <summary>Unpermitted alteration to the property.</summary>
    UnpermittedAlteration = 1,

    /// <summary>Violation related to the physical condition of the property.</summary>
    PhysicalCondition = 2,

    /// <summary>Other lease term violation.</summary>
    LeaseTermViolation = 3
}
