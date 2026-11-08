using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.Resales.Events;

/// <summary>Raised when the trust has paid for and taken the home, ending the resale.</summary>
/// <param name="AggregateId">The resale's ID.</param>
/// <param name="GroundLeaseId">The seller's lease, which ends with the closing.</param>
/// <param name="HomeId">The home the trust bought.</param>
/// <param name="ClosingDate">The day the buyback closed.</param>
/// <param name="PricePaid">What the trust paid.</param>
/// <param name="Notes">Anything staff noted about the closing.</param>
/// <param name="OfferedForSale">Whether the home goes to the waiting list now, rather than being held by the trust.</param>
/// <param name="HoldReason">Why the trust is holding the home, when it is.</param>
/// <param name="ClosedBy">The user who recorded the closing.</param>
public sealed record BuybackClosed(
    string AggregateId,
    string GroundLeaseId,
    string HomeId,
    DateOnly ClosingDate,
    decimal PricePaid,
    string? Notes,
    bool OfferedForSale,
    string? HoldReason,
    string ClosedBy) : DomainEvent(AggregateId);
