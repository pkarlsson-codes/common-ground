using CommonGround.SharedKernel.Domain;

namespace CommonGround.ApplicantSelection.Domain.HomeownerApplications;

/// <summary>Strongly-typed identifier for a homeowner application.</summary>
public sealed class HomeownerApplicationId(string value)
    : EntityId<HomeownerApplicationId>(value, Prefix)
{
    private const string Prefix = "application";
}