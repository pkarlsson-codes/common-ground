using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Contracts.Events;

/// <summary>
/// Integration event published when where a home stands in being sold changes. Consumed by HomeBuyers, which chooses
/// a household for a home only while it is offered. News can arrive out of order, so a consumer keeps the one with
/// the highest <paramref name="Sequence"/>.
/// </summary>
/// <param name="HomeId">The home's ID.</param>
/// <param name="SaleStage">The home's sale stage: NotForSale, AwaitingTrustDecision, Offered, UnderContract or HeldByTrust.</param>
/// <param name="Since">The date the home reached the stage, or null when it has been there since it was registered.</param>
/// <param name="Sequence">Counts the home's stage changes, so a later one has a higher number.</param>
/// <param name="LatestSelectionId">
/// The latest home selection PropertyRegistry had heard of when the stage changed, or null when it had heard of none,
/// so HomeBuyers can tell whether the stage already accounts for its latest choice.
/// </param>
public sealed record HomeSaleStageChanged(
    string HomeId,
    string SaleStage,
    DateOnly? Since,
    int Sequence,
    string? LatestSelectionId) : IntegrationEvent;
