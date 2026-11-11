using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.LeaseFeeNoticePolicies.Events;

/// <summary>Raised when a trust sets its lease fee notice policy, the first time or any time after.</summary>
/// <param name="AggregateId">The policy's ID.</param>
/// <param name="FirstNoticeDays">Days overdue before the first notice.</param>
/// <param name="SecondNoticeDays">Days overdue before the second notice.</param>
/// <param name="DefaultNoticeDays">Days overdue before the notice of default.</param>
/// <param name="CureDays">Days a notice of default gives to pay.</param>
/// <param name="CureExtensionDays">Days added when two-thirds was paid within the cure days.</param>
/// <param name="LetterSignOff">The lines printed at the end of every notice letter; empty for none.</param>
/// <param name="SetBy">The user who set it.</param>
public sealed record LeaseFeeNoticePolicySet(
    string AggregateId,
    int FirstNoticeDays,
    int SecondNoticeDays,
    int DefaultNoticeDays,
    int CureDays,
    int CureExtensionDays,
    string LetterSignOff,
    string SetBy) : DomainEvent(AggregateId);
