namespace CommonGround.HomeBuyers.Application.ReadModels;

/// <summary>
/// HomeBuyers' local copy of the home facts that selection needs. Seeded (eventually consistent)
/// from PropertyRegistry's HomeRegistered integration event. Resolves LandParcelId server-side
/// for applicant selection, and gives selection the home's size and accessibility to compare
/// candidates against.
/// </summary>
public class HomeFactsView
{
    /// <summary>Gets or sets the home ID (the identity of this view).</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the land parcel ID the home sits on.</summary>
    public string LandParcelId { get; set; } = string.Empty;

    /// <summary>Gets or sets the label staff know the home by (its parcel address plus any unit designation).</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>Gets or sets the number of bedrooms.</summary>
    public int Bedrooms { get; set; }

    /// <summary>Gets or sets a value indicating whether the home is accessible to people with disabilities.</summary>
    public bool IsAccessible { get; set; }
}
