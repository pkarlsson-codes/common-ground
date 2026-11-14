using System.Text.Json.Serialization;

namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to change the insurance a lease requires from a day on (Art 9.4).</summary>
/// <param name="ReplacementValue">The replacement value the dwelling coverage must reach, zero or more; null stops checking it.</param>
/// <param name="LiabilityPerOccurrence">The liability coverage per occurrence required, zero or more; null stops checking it.</param>
/// <param name="EffectiveFrom">The first day they apply, on or after the lease start and the latest change.</param>
public sealed record SetInsuranceRequirementsRequest(
    [property: JsonRequired] decimal? ReplacementValue,
    [property: JsonRequired] decimal? LiabilityPerOccurrence,
    DateOnly EffectiveFrom);
