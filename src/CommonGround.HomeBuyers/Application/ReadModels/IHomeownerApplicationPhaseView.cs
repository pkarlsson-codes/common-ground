namespace CommonGround.HomeBuyers.Application.ReadModels;

/// <summary>A homeowner application read model that stores the application's phase.</summary>
public interface IHomeownerApplicationPhaseView
{
    /// <summary>Gets the <see cref="Domain.HomeownerApplications.ApplicationPhase"/> display name.</summary>
    string Phase { get; }
}
