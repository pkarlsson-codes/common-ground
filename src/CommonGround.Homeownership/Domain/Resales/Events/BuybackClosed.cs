using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.Resales.Events;

/// <summary>Raised when the trust has paid for and taken the home, ending the resale.</summary>
/// <param name="GroundLeaseId">The seller's lease, which ends with the closing.</param>
/// <param name="ClosingDate">The day the buyback closed.</param>
/// <param name="OfferedForSale">Whether the home goes to the waiting list now, rather than being held by the trust.</param>
/// <param name="HoldReason">Why the trust is holding the home, when it is.</param>
/// <param name="FeesPaidFromProceeds">Lease fees paid to the trust from the seller's proceeds at the closing; null for none.</param>
public sealed record BuybackClosed(
    string AggregateId,
    string GroundLeaseId,
    string HomeId,
    DateOnly ClosingDate,
    Money PricePaid,
    string? Notes,
    bool OfferedForSale,
    string? HoldReason,
    string ClosedBy,
    Money? FeesPaidFromProceeds = null) : DomainEvent(AggregateId);
