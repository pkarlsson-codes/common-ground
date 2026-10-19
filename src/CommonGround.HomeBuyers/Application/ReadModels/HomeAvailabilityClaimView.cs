using CommonGround.HomeBuyers.Domain.HomeownerApplications;

namespace CommonGround.HomeBuyers.Application.ReadModels;

/// <summary>
/// Tracks which <see cref="HomeownerApplication"/>
/// has claimed a given home's availability. One doc per HomeId; existence of a second, different
/// claimant is the concurrency guard against selecting two applicants for the same home.
/// </summary>
public class HomeAvailabilityClaimView
{
    /// <summary>Gets or sets the home ID (the identity of this view).</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the ID of the application that claimed this home.</summary>
    public string ApplicationId { get; set; } = string.Empty;
}
