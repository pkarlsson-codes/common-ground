using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.LeaseFeeNoticePolicies.Events;

/// <summary>Raised when a trust sets its lease fee notice policy, the first time or any time after.</summary>
/// <param name="AggregateId">The policy's ID.</param>
/// <param name="FirstNoticeDays">Days overdue before the first notice.</param>
/// <param name="SecondNoticeDays">Days overdue before the second notice.</param>
/// <param name="LetterSignOff">The lines printed at the end of every notice letter; empty for none.</param>
/// <param name="SetBy">The user who set it.</param>
public sealed record LeaseFeeNoticePolicySet(
    string AggregateId,
    int FirstNoticeDays,
    int SecondNoticeDays,
    string LetterSignOff,
    string SetBy) : DomainEvent(AggregateId);
