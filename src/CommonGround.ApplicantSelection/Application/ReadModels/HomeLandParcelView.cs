namespace CommonGround.ApplicantSelection.Application.ReadModels;

/// <summary>
/// Maps a home to its land parcel. Seeded (eventually consistent) from PropertyRegistry's
/// HomeRegistered integration event. Backs the candidate-list query and resolves
/// LandParcelId server-side for applicant selection.
/// </summary>
public class HomeLandParcelView
{
    /// <summary>Gets or sets the home ID (the identity of this view).</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the land parcel ID the home sits on.</summary>
    public string LandParcelId { get; set; } = string.Empty;
}
