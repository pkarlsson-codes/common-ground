namespace CommonGround.HomeBuyers.Application.ReadModels;

/// <summary>
/// HomeBuyers' local copy of the home facts that selection needs. Seeded (eventually consistent)
/// from PropertyRegistry's HomeRegistered and HomeIncomeLimitSet integration events. Resolves
/// LandParcelId server-side for applicant selection, and gives selection the home's size,
/// accessibility and income limit to compare candidates against.
/// </summary>
public class HomeFactsView
{
    public string Id { get; set; } = string.Empty;

    public string LandParcelId { get; set; } = string.Empty;

    /// <summary>Gets or sets the label staff know the home by (its parcel address plus any unit designation).</summary>
    public string Label { get; set; } = string.Empty;

    public int Bedrooms { get; set; }

    public bool IsAccessible { get; set; }

    /// <summary>Gets or sets the income limit as a percent of AMI, or null when the home has no income limit.</summary>
    public int? IncomeLimitPercentOfAmi { get; set; }
}
