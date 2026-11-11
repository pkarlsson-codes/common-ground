using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeases.Events;

/// <summary>Raised when staff record that a late lease fee notice was sent.</summary>
/// <param name="AggregateId">The ground lease ID.</param>
/// <param name="NoticeId">The notice's ID.</param>
/// <param name="Level">Which notice it was.</param>
/// <param name="SentOn">The day it was sent.</param>
/// <param name="Delivery">How it was sent.</param>
/// <param name="BalanceDue">What the lease owed that day.</param>
/// <param name="OldestUnpaidDueDate">The due date of the oldest unpaid charge that day.</param>
/// <param name="RecordedBy">The staff user who recorded it.</param>
/// <param name="CureDays">A notice of default's cure days; null for other notices.</param>
/// <param name="CureExtensionDays">A notice of default's extension days; null for other notices.</param>
public sealed record LeaseFeeNoticeRecorded(
    string AggregateId,
    string NoticeId,
    LeaseFeeNoticeLevel Level,
    DateOnly SentOn,
    NoticeDelivery Delivery,
    decimal BalanceDue,
    DateOnly OldestUnpaidDueDate,
    string RecordedBy,
    int? CureDays = null,
    int? CureExtensionDays = null) : DomainEvent(AggregateId);
