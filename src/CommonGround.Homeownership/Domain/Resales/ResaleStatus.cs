using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.Resales;

/// <summary>Where a resale stands.</summary>
public sealed class ResaleStatus : Enumeration<ResaleStatus>
{
    /// <summary>The homeowner has given notice and the resale is in progress.</summary>
    public static readonly ResaleStatus Open = new(1, "Open");

    /// <summary>The homeowner decided not to sell after all.</summary>
    public static readonly ResaleStatus Withdrawn = new(2, "Withdrawn");

    private ResaleStatus(int value, string displayName) : base(value, displayName) { }
}
