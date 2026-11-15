namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to record that the trust paid for and took a home it bought back.</summary>
/// <param name="ClosingDate">The day the buyback closed.</param>
/// <param name="PricePaid">What the trust paid.</param>
/// <param name="Notes">Anything staff want to note about the closing.</param>
/// <param name="OfferForSale">Whether the home goes to the waiting list now, rather than being held.</param>
/// <param name="HoldReason">Why the trust is holding the home; required when it is held.</param>
/// <param name="FeesPaidFromProceeds">Lease fees the closing agent paid the trust from the seller's proceeds; absent or zero for none.</param>
public sealed record RecordBuybackClosedRequest(
    DateOnly ClosingDate,
    decimal PricePaid,
    string? Notes,
    bool OfferForSale,
    string? HoldReason,
    decimal? FeesPaidFromProceeds = null);
