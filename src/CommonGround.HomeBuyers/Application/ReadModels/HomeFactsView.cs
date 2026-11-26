namespace CommonGround.HomeBuyers.Application.ReadModels;

/// <summary>
/// HomeBuyers' local copy of the home facts that selection needs. Seeded (eventually consistent)
/// from PropertyRegistry's HomeRegistered, HomeIncomeLimitSet and HomeSaleStageChanged integration events.
/// Resolves LandParcelId server-side for applicant selection, gives selection the home's size,
/// accessibility and income limit to compare the waiting list against, and says whether a household can be
/// chosen for it now. Stored with optimistic concurrency, so two choices for the same home can't both succeed.
/// </summary>
public class HomeFactsView
{
    public string Id { get; set; } = string.Empty;

    public string LandParcelId { get; set; } = string.Empty;

    /// <summary>Gets or sets the label staff know the home by (its parcel address plus any unit designation).</summary>
    public string Label { get; set; } = string.Empty;

    public int Bedrooms { get; set; }

    public decimal Bathrooms { get; set; }

    public decimal SquareFootage { get; set; }

    public bool IsAccessible { get; set; }

    /// <summary>Gets or sets the income limit as a percent of AMI, or null when the home has no income limit.</summary>
    public int? IncomeLimitPercentOfAmi { get; set; }

    /// <summary>
    /// Gets or sets where the home stands in being sold, as PropertyRegistry last published it: NotForSale,
    /// AwaitingTrustDecision, Offered, UnderContract or HeldByTrust. A home PropertyRegistry hasn't published one for is offered.
    /// </summary>
    public string SaleStage { get; set; } = OfferedStage;

    /// <summary>Gets or sets the date the home reached <see cref="SaleStage"/>, or null when it has been there since it was registered.</summary>
    public DateOnly? SaleStageSince { get; set; }

    /// <summary>Gets or sets the sequence number of the sale stage last published, so older news arriving late is ignored.</summary>
    public int SaleStageSequence { get; set; }

    /// <summary>Gets or sets the latest selection PropertyRegistry had heard of when it published <see cref="SaleStage"/>.</summary>
    public string? SaleStageSelectionId { get; set; }

    /// <summary>Gets or sets the latest selection HomeBuyers recorded for the home, or null when there has been none.</summary>
    public string? LatestSelectionId { get; set; }

    /// <summary>
    /// Gets a value indicating whether a household can be chosen for the home now: it is offered, and the stage
    /// already accounts for HomeBuyers' latest choice. Until PropertyRegistry hears of a choice, the home counts as chosen.
    /// </summary>
    public bool IsOffered => SaleStage == OfferedStage && LatestSelectionId == SaleStageSelectionId;

    /// <summary>
    /// Gets where the home stands, counting a choice PropertyRegistry hasn't heard of yet as putting it under contract.
    /// </summary>
    public string CurrentSaleStage => LatestSelectionId == SaleStageSelectionId ? SaleStage : UnderContractStage;

    /// <summary>The sale stage of a home offered to the waiting list.</summary>
    public const string OfferedStage = "Offered";

    /// <summary>The sale stage of a home a household was chosen for.</summary>
    public const string UnderContractStage = "UnderContract";
}
