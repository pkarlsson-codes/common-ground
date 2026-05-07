using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeases;

/// <summary>Status of a ground lease.</summary>
public sealed class GroundLeaseStatus : Enumeration<GroundLeaseStatus>
{
    /// <summary>The ground lease is active.</summary>
    public static readonly GroundLeaseStatus Active = new(0, "Active");

    /// <summary>The ground lease is inactive.</summary>
    public static readonly GroundLeaseStatus Inactive = new(1, "Inactive");

    private GroundLeaseStatus(int value, string displayName) : base(value, displayName) { }
}
