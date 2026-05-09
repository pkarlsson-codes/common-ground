using System.Text.Json.Serialization;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.Stewardship.Domain.StewardshipCases;

/// <summary>The category of a lease violation.</summary>
public sealed class LeaseViolationType : Enumeration<LeaseViolationType>
{
    /// <summary>Violation related to primary occupancy requirements.</summary>
    public static readonly LeaseViolationType Occupancy = new(0, "Occupancy");

    /// <summary>Unpermitted alteration to the property.</summary>
    public static readonly LeaseViolationType UnpermittedAlteration = new(1, "UnpermittedAlteration");

    /// <summary>Violation related to the physical condition of the property.</summary>
    public static readonly LeaseViolationType PhysicalCondition = new(2, "PhysicalCondition");

    /// <summary>Other lease term violation.</summary>
    public static readonly LeaseViolationType LeaseTermViolation = new(3, "LeaseTermViolation");

    [JsonConstructor]
    private LeaseViolationType(int value, string displayName) : base(value, displayName) { }
}
