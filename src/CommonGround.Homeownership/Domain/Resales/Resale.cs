using CommonGround.Homeownership.Application.Errors;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.Resales.Events;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.Resales;

/// <summary>
/// The seller's side of a home changing hands: from the homeowner's notice of intent to sell, through the
/// trust's purchase option window. The lease it belongs to allows one open resale at a time.
/// </summary>
public sealed class Resale : AggregateRoot<ResaleId>
{
#pragma warning disable CS8618 // State is set by Create from the stream's first event.
    private Resale() { }
#pragma warning restore CS8618

    /// <summary>Gets the lease of the homeowner selling.</summary>
    public GroundLeaseId GroundLeaseId { get; private set; }

    /// <summary>Gets the home being sold.</summary>
    public string HomeId { get; private set; }

    /// <summary>Gets the day the homeowner gave notice.</summary>
    public DateOnly NoticeDate { get; private set; }

    /// <summary>Gets the last day of the trust's purchase option.</summary>
    public DateOnly OptionWindowEndsOn { get; private set; }

    /// <summary>Gets where the resale stands.</summary>
    public ResaleStatus Status { get; private set; }

    /// <summary>
    /// Opens a resale from a homeowner's notice of intent to sell. The trust's option window runs for the
    /// lease's option window days from the notice date. The lease decides whether a resale may open; see
    /// <see cref="GroundLease.StartResale"/>.
    /// </summary>
    public static IntentToSellRecorded RecordIntentToSell(
        ResaleId id,
        GroundLease lease,
        DateOnly noticeDate,
        NoticeChannel channel,
        string? notes,
        DateOnly today,
        UserId recordedBy)
    {
        ArgumentNullException.ThrowIfNull(lease);
        ArgumentNullException.ThrowIfNull(channel);

        if (noticeDate > today)
            throw BusinessRuleViolation.Resale.RecordIntentToSell.NoticeDateInFuture(noticeDate, today);

        return new IntentToSellRecorded(
            id.Value,
            lease.Id.Value,
            lease.LandParcelId,
            lease.UnitId,
            lease.CurrentHomeowner?.FullName.Value,
            noticeDate,
            channel.DisplayName,
            string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            noticeDate.AddDays(lease.OptionWindowDays),
            recordedBy.Value);
    }

    /// <summary>Withdraws the resale because the homeowner decided not to sell.</summary>
    public ResaleWithdrawn Withdraw(DateOnly withdrawnDate, string reason, DateOnly today, UserId withdrawnBy)
    {
        if (Status != ResaleStatus.Open)
            throw BusinessRuleViolation.Resale.Withdraw.NotOpen(Status);

        if (string.IsNullOrWhiteSpace(reason))
            throw BusinessRuleViolation.Resale.Withdraw.ReasonRequired();

        if (withdrawnDate < NoticeDate)
            throw BusinessRuleViolation.Resale.Withdraw.WithdrawnDateBeforeNotice(withdrawnDate, NoticeDate);

        if (withdrawnDate > today)
            throw BusinessRuleViolation.Resale.Withdraw.WithdrawnDateInFuture(withdrawnDate, today);

        return new ResaleWithdrawn(Id.Value, GroundLeaseId.Value, HomeId, withdrawnDate, reason.Trim(), withdrawnBy.Value);
    }

    /// <summary>Evolves a new resale from its stream's first event.</summary>
    public static Resale Create(IntentToSellRecorded e)
    {
        ArgumentNullException.ThrowIfNull(e);

        return new Resale
        {
            Id = ResaleId.Create(e.AggregateId),
            GroundLeaseId = GroundLeaseId.Create(e.GroundLeaseId),
            HomeId = e.HomeId,
            NoticeDate = e.NoticeDate,
            OptionWindowEndsOn = e.OptionWindowEndsOn,
            Status = ResaleStatus.Open,
        };
    }

    /// <summary>Applies <see cref="ResaleWithdrawn"/>.</summary>
    public void Apply(ResaleWithdrawn _) => Status = ResaleStatus.Withdrawn;
}
