using CommonGround.SharedKernel.Domain;

namespace CommonGround.PropertyRegistry.Application.ReadModels;

/// <summary>Who has a home now, as <see cref="HomeAvailabilityView"/> shows it.</summary>
public sealed class HomeOccupancy : Enumeration<HomeOccupancy>
{
    /// <summary>The trust has the home; no homeowner leases it.</summary>
    public static readonly HomeOccupancy Vacant = new(1, "Vacant");

    /// <summary>A homeowner leases the home, from the lease start date.</summary>
    public static readonly HomeOccupancy Occupied = new(2, "Occupied");

    private HomeOccupancy(int value, string displayName) : base(value, displayName) { }
}
