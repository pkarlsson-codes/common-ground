using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.MortgageRequests.Events;

/// <summary>Raised when the trust refuses a requested loan. The reason goes in the denial letter.</summary>
/// <param name="DeniedOn">The day the trust refused it.</param>
public sealed record MortgageRequestDenied(
    string AggregateId,
    string GroundLeaseId,
    DateOnly DeniedOn,
    string Reason,
    string DeniedBy) : DomainEvent(AggregateId);
