using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeases.Events;

/// <summary>Raised when staff correct the day a lease's fees are tracked from, or what was owed on it.</summary>
/// <param name="FeesTrackedFrom">The new first day the trust tracks fees in CommonGround.</param>
/// <param name="OpeningBalance">The new amount owed on that day.</param>
public sealed record LeaseFeeTrackingCorrected(
    string AggregateId,
    DateOnly FeesTrackedFrom,
    Money OpeningBalance,
    string CorrectedBy) : DomainEvent(AggregateId);
