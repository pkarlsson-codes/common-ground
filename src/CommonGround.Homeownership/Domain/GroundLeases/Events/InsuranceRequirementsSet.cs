using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeases.Events;

/// <summary>Raised when staff change the insurance a lease requires (Art 9.4).</summary>
/// <param name="AggregateId">The ground lease.</param>
/// <param name="EffectiveFrom">The first day the new requirements apply.</param>
/// <param name="ReplacementValue">The replacement value the dwelling coverage must reach, or null when not checked.</param>
/// <param name="LiabilityPerOccurrence">The liability coverage per occurrence required, or null when not checked.</param>
/// <param name="SetBy">The staff user who set them.</param>
public sealed record InsuranceRequirementsSet(
    string AggregateId,
    DateOnly EffectiveFrom,
    Money? ReplacementValue,
    Money? LiabilityPerOccurrence,
    string SetBy) : DomainEvent(AggregateId);
