using CommonGround.SharedKernel.Domain;

namespace CommonGround.PropertyRegistry.Domain.Homes;

/// <summary>Where a home is in its lifecycle, from offering it to the waiting list through leasing and resale.</summary>
public sealed class HomeStatus : Enumeration<HomeStatus>
{
    /// <summary>Home is vacant and available for a new homeowner.</summary>
    public static readonly HomeStatus Available = new(1, "Available");

    /// <summary>A home selection is choosing a household from the waiting list for the home.</summary>
    public static readonly HomeStatus InSelection = new(2, "InSelection");

    /// <summary>Home's purchase has closed and it is leased to its homeowner.</summary>
    public static readonly HomeStatus Leased = new(3, "Leased");

    /// <summary>Home is being offered for resale through the CLT resale process.</summary>
    public static readonly HomeStatus Resale = new(4, "Resale");

    /// <summary>Home has a pending sale and is under contract to a new homeowner.</summary>
    public static readonly HomeStatus UnderContract = new(5, "UnderContract");

    private HomeStatus(int value, string displayName) : base(value, displayName) { }
}
