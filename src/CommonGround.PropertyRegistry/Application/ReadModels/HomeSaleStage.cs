using CommonGround.SharedKernel.Domain;

namespace CommonGround.PropertyRegistry.Application.ReadModels;

/// <summary>Where a home is in being sold, as <see cref="HomeAvailabilityView"/> shows it.</summary>
public sealed class HomeSaleStage : Enumeration<HomeSaleStage>
{
    /// <summary>The home is not being sold.</summary>
    public static readonly HomeSaleStage NotForSale = new(1, "NotForSale");

    /// <summary>A homeowner gave notice to sell and the trust has not yet decided whether to buy.</summary>
    public static readonly HomeSaleStage AwaitingTrustDecision = new(2, "AwaitingTrustDecision");

    /// <summary>The home is ready for a selection to choose a household.</summary>
    public static readonly HomeSaleStage Offered = new(3, "Offered");

    /// <summary>A home selection is choosing a household from the waiting list.</summary>
    public static readonly HomeSaleStage InSelection = new(4, "InSelection");

    /// <summary>A selection chose a household and the sale is pending.</summary>
    public static readonly HomeSaleStage UnderContract = new(5, "UnderContract");

    private HomeSaleStage(int value, string displayName) : base(value, displayName) { }
}
