using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.HomeSelections.Events;

/// <summary>
/// Staff chose a household for <paramref name="HomeId"/> on <paramref name="SelectedDate"/>, citing the
/// <see cref="SelectionPriority"/> values in <paramref name="Priorities"/> and explaining the choice in
/// <paramref name="Reason"/>. <paramref name="Applications"/> is the whole waiting list at that moment, each marked
/// chosen, passed over or couldn't be chosen. <paramref name="IncomeLimitPercentOfAmi"/> is the home's income limit
/// they were judged against, null when it has none. <paramref name="IncomeLimitsEffectiveFrom"/> and
/// <paramref name="IncomeLimitLevels"/> are the income limit schedule in effect on the date, every level of it, so the
/// purchase can judge a household whose size changed and record its income band; both are null when none was.
/// </summary>
public sealed record HouseholdSelectedByPriority(
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
    IReadOnlyList<SelectionPriority> Priorities,
    string Reason) : DomainEvent(AggregateId);
