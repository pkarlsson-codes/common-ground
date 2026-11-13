using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.ForeclosurePreventionCases.Events;

/// <summary>Raised when a case opened from a lease default records the unpaid notice of default that led to it.</summary>
/// <param name="AggregateId">The foreclosure prevention case identifier.</param>
/// <param name="NoticeSentOn">The day the notice of default was sent.</param>
/// <param name="AmountOwed">What the notice said was owed.</param>
/// <param name="CureBy">The last day to pay the amount owed, after any extension.</param>
/// <param name="StillToPay">What was still to pay of the amount owed when the case opened.</param>
/// <param name="RecordedDate">The trust date the arrears were recorded.</param>
/// <param name="RecordedBy">The staff member who opened the case.</param>
public sealed record LeaseFeeArrearsRecorded(
    string AggregateId,
    DateOnly NoticeSentOn,
    decimal AmountOwed,
    DateOnly CureBy,
    decimal StillToPay,
    DateOnly RecordedDate,
    string RecordedBy) : DomainEvent(AggregateId);
