using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.HomeSelections.Events;

/// <summary>
/// A household was drawn by lottery for <paramref name="HomeId"/> on <paramref name="SelectedDate"/>.
/// <paramref name="TiedApplicationIds"/> are the applications staff found tied, null when the draw was over the whole
/// waiting list. <paramref name="PoolApplicationIds"/> are those of them that could be chosen, which the draw was
/// over; drawing again with <paramref name="Seed"/> over the same pool gives the same winner.
/// <paramref name="Applications"/> is the whole waiting list at that moment, each marked chosen, passed over or couldn't
/// be chosen. The income limit fields are as on <see cref="HouseholdSelectedByPriority"/>.
/// </summary>
public sealed record HouseholdSelectedByLottery(
    string AggregateId,
    string HomeId,
    string ApplicationId,
    DateOnly SelectedDate,
    string SelectedBy,
    string GroundLeaseTemplateId,
    decimal PurchasePrice,
    int? IncomeLimitPercentOfAmi,
    DateOnly? IncomeLimitsEffectiveFrom,
    IReadOnlyList<IncomeLimitLevelEntry>? IncomeLimitLevels,
    IReadOnlyList<RecordedApplication> Applications,
    IReadOnlyList<string>? TiedApplicationIds,
    IReadOnlyList<string> PoolApplicationIds,
    int Seed) : DomainEvent(AggregateId);
