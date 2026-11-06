using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.Resales.Events;

/// <summary>Raised when a homeowner who gave notice to sell decides not to.</summary>
/// <param name="AggregateId">The resale's ID.</param>
/// <param name="GroundLeaseId">The lease of the homeowner who withdrew.</param>
/// <param name="HomeId">The home no longer being sold.</param>
/// <param name="WithdrawnDate">The day the homeowner withdrew.</param>
/// <param name="Reason">Why the homeowner withdrew.</param>
/// <param name="WithdrawnBy">The user who recorded the withdrawal.</param>
public sealed record ResaleWithdrawn(
    string AggregateId,
    string GroundLeaseId,
    string HomeId,
    DateOnly WithdrawnDate,
    string Reason,
    string WithdrawnBy) : DomainEvent(AggregateId);
