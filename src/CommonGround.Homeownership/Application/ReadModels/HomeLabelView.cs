namespace CommonGround.Homeownership.Application.ReadModels;

/// <summary>
/// Homeownership's local copy of the label staff know a home by (its parcel address plus any unit designation).
/// Seeded (eventually consistent) from PropertyRegistry's HomeRegistered integration event.
/// </summary>
public class HomeLabelView
{
    /// <summary>Gets or sets the home ID (the identity of this view).</summary>
    public string Id { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;
}
