using CommonGround.Homeownership.Application.Errors;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.HeirTransfers.Events;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.HeirTransfers;

/// <summary>
/// The passing of a deceased homeowner's home to an heir (Model Lease Art 10.3). The executor tells the trust within
/// 90 days; the trust records who is taking the home and how they qualify, then consents once the heir has signed the
/// Letter of Agreement and the Attorney's Acknowledgment. The lease stays: its base price, fee ledger, mortgages and
/// insurance carry on with the heir as homeowner. A lease has at most one open transfer.
/// </summary>
public sealed class HeirTransfer : AggregateRoot<HeirTransferId>
{
    /// <summary>The days after the death the lease gives the executor to tell the trust (Art 10.3).</summary>
    public const int NoticeDays = 90;

    /// <summary>The time a household member must have lived in the home before the death to qualify without an income check, in years.</summary>
    public const int HouseholdMemberYears = 1;

#pragma warning disable CS8618 // State is set by Create from the stream's first event.
    private HeirTransfer() { }
#pragma warning restore CS8618

    /// <summary>Gets the lease the home is held on.</summary>
    public GroundLeaseId GroundLeaseId { get; private set; }

    /// <summary>Gets where the transfer stands.</summary>
    public HeirTransferStatus Status { get; private set; }

    /// <summary>Gets the day the homeowner died.</summary>
    public DateOnly DiedOn { get; private set; }

    /// <summary>Gets the day the executor told the trust.</summary>
    public DateOnly NoticeReceivedOn { get; private set; }

    /// <summary>Gets whether the executor told the trust more than <see cref="NoticeDays"/> after the death.</summary>
    public bool NoticeWasLate => NoticeReceivedOn > DiedOn.AddDays(NoticeDays);

    /// <summary>Gets the heir, once recorded.</summary>
    public Homeowner? Heir { get; private set; }

    /// <summary>Gets how the heir qualifies, once recorded.</summary>
    public HeirRelationship? Relationship { get; private set; }

    /// <summary>Gets the day the home passed to the heir, once completed.</summary>
    public DateOnly? TransferredOn { get; private set; }

    /// <summary>
    /// Opens a transfer when the executor tells the trust of the homeowner's death. Refused unless the lease is active
    /// and has no other open transfer, for a death or notice date in the future, a notice before the death, or no
    /// name for who gave notice. A notice more than <see cref="NoticeDays"/> after the death is recorded, not refused.
    /// </summary>
    /// <param name="id">The new transfer's ID.</param>
    /// <param name="lease">The lease the home is held on.</param>
    /// <param name="openTransferId">The lease's open transfer, if it has one.</param>
    /// <param name="diedOn">The day the homeowner died.</param>
    /// <param name="noticeReceivedOn">The day the executor told the trust.</param>
    /// <param name="notifiedBy">Who told the trust.</param>
    /// <param name="notifiedByContact">How to reach them, if given.</param>
    /// <param name="notes">Anything staff want to note.</param>
    /// <param name="today">The trust's today.</param>
    /// <param name="openedBy">The staff user opening it.</param>
    public static HeirTransferOpened Open(
        HeirTransferId id,
        GroundLease lease,
        HeirTransferId? openTransferId,
        DateOnly diedOn,
        DateOnly noticeReceivedOn,
        string notifiedBy,
        string? notifiedByContact,
        string? notes,
        DateOnly today,
        UserId openedBy)
    {
        ArgumentNullException.ThrowIfNull(lease);

        if (lease.Status != GroundLeaseStatus.Active)
            throw BusinessRuleViolation.HeirTransfer.Open.LeaseNotActive(lease.Status);

        if (openTransferId is { } open)
            throw BusinessRuleViolation.HeirTransfer.Open.AlreadyOpen(open.Value);

        if (string.IsNullOrWhiteSpace(notifiedBy))
            throw BusinessRuleViolation.HeirTransfer.Open.NotifiedByRequired();

        if (diedOn > today)
            throw BusinessRuleViolation.HeirTransfer.Open.DiedOnInFuture(diedOn, today);

        if (noticeReceivedOn < diedOn)
            throw BusinessRuleViolation.HeirTransfer.Open.NoticeBeforeDeath(noticeReceivedOn, diedOn);

        if (noticeReceivedOn > today)
            throw BusinessRuleViolation.HeirTransfer.Open.NoticeInFuture(noticeReceivedOn, today);

        return new HeirTransferOpened(
            id.Value,
            lease.Id.Value,
            lease.UnitId,
            lease.CurrentHomeowner?.FullName.Value,
            diedOn,
            noticeReceivedOn,
            notifiedBy.Trim(),
            Trimmed(notifiedByContact),
            Trimmed(notes),
            openedBy.Value);
    }

    /// <summary>
    /// Records who is taking the home and how they qualify. A spouse or child needs nothing more. A household member
    /// must have moved in at least <see cref="HouseholdMemberYears"/> year before the death. Any other heir needs a
    /// verified income and household size, income documents, and an income under the home's limit for the household
    /// size; a home without a limit takes the heir unchecked. Recording again replaces the heir.
    /// </summary>
    /// <param name="fullName">The heir's full name.</param>
    /// <param name="email">The heir's email address.</param>
    /// <param name="phone">The heir's phone number, if given.</param>
    /// <param name="relationship">How the heir qualifies.</param>
    /// <param name="movedInOn">The day a household member moved in; read for that relationship only.</param>
    /// <param name="verifiedAnnualIncome">The heir's verified annual income; read for any other heir only.</param>
    /// <param name="verifiedHouseholdSize">The heir's household size; read for any other heir only.</param>
    /// <param name="incomeDocumentIds">The income documents; read for any other heir only.</param>
    /// <param name="incomeLimit">The home's income limit on the notice date, or null when it has none.</param>
    /// <param name="recordedBy">The staff user recording it.</param>
    public HeirRecorded RecordHeir(
        string? fullName,
        string? email,
        string? phone,
        HeirRelationship relationship,
        DateOnly? movedInOn,
        Money? verifiedAnnualIncome,
        int? verifiedHouseholdSize,
        IReadOnlyList<string>? incomeDocumentIds,
        HeirIncomeLimit? incomeLimit,
        UserId recordedBy)
    {
        ArgumentNullException.ThrowIfNull(relationship);

        if (Status != HeirTransferStatus.Open && Status != HeirTransferStatus.HeirRecorded)
            throw BusinessRuleViolation.HeirTransfer.RecordHeir.NotOpen(Status);

        var heir = Homeowner.From(fullName, email, phone);

        if (relationship == HeirRelationship.HouseholdMember)
            CheckHouseholdMember(movedInOn);

        if (relationship == HeirRelationship.Other)
        {
            var (income, size, documents, limit) = CheckOtherHeir(verifiedAnnualIncome, verifiedHouseholdSize, incomeDocumentIds, incomeLimit);

            return new HeirRecorded(
                Id.Value, GroundLeaseId.Value, heir.FullName.Value, heir.Email.Value, heir.Phone?.Value, relationship,
                null, income, size, documents, limit, limit is null ? null : incomeLimit!.PercentOfAmi,
                limit is null ? null : incomeLimit!.Schedule.EffectiveFrom, recordedBy.Value);
        }

        return new HeirRecorded(
            Id.Value, GroundLeaseId.Value, heir.FullName.Value, heir.Email.Value, heir.Phone?.Value, relationship,
            relationship == HeirRelationship.HouseholdMember ? movedInOn : null,
            null, null, [], null, null, null, recordedBy.Value);
    }

    private void CheckHouseholdMember(DateOnly? movedInOn)
    {
        if (movedInOn is not { } movedIn)
            throw BusinessRuleViolation.HeirTransfer.RecordHeir.MovedInOnRequired();

        if (movedIn > DiedOn.AddYears(-HouseholdMemberYears))
            throw BusinessRuleViolation.HeirTransfer.RecordHeir.HouseholdMemberUnderOneYear(movedIn, DiedOn);
    }

    private static (Money Income, int Size, IReadOnlyList<string> Documents, Money? Limit) CheckOtherHeir(
        Money? verifiedAnnualIncome, int? verifiedHouseholdSize, IReadOnlyList<string>? incomeDocumentIds, HeirIncomeLimit? incomeLimit)
    {
        if (verifiedAnnualIncome is not { } income)
            throw BusinessRuleViolation.HeirTransfer.RecordHeir.IncomeRequired();

        if (verifiedHouseholdSize is not { } size)
            throw BusinessRuleViolation.HeirTransfer.RecordHeir.HouseholdSizeRequired();

        if (incomeDocumentIds is null || incomeDocumentIds.Count == 0 || incomeDocumentIds.Any(string.IsNullOrWhiteSpace))
            throw BusinessRuleViolation.HeirTransfer.RecordHeir.IncomeDocumentsRequired();

        if (income.IsNegative)
            throw BusinessRuleViolation.HeirTransfer.RecordHeir.IncomeNegative(income.Amount);

        if (size < 1)
            throw BusinessRuleViolation.HeirTransfer.RecordHeir.HouseholdSizeBelowOne(size);

        var limit = incomeLimit?.LimitFor(size);
        if (limit is not null && income > limit)
        {
            throw BusinessRuleViolation.HeirTransfer.RecordHeir.OverIncomeLimit(
                income.Amount, limit.Amount, incomeLimit!.PercentOfAmi, size, incomeLimit.Schedule.EffectiveFrom);
        }

        return (income, size, [.. incomeDocumentIds.Select(d => d.Trim()).Distinct()], limit);
    }

    /// <summary>
    /// Records the trust's consent once the heir has signed the Letter of Agreement and the Attorney's Acknowledgment.
    /// Refused unless an heir is recorded, without both letters, or for a date before the death or in the future.
    /// </summary>
    /// <param name="transferredOn">The day the home passed to the heir.</param>
    /// <param name="letterOfAgreementDocumentId">The signed Letter of Agreement, as a Documents file ID.</param>
    /// <param name="attorneyAcknowledgementDocumentId">The signed Attorney's Acknowledgment, as a Documents file ID.</param>
    /// <param name="notes">Anything staff want to note.</param>
    /// <param name="today">The trust's today.</param>
    /// <param name="completedBy">The staff user recording it.</param>
    public HeirTransferCompleted Complete(
        DateOnly transferredOn,
        string? letterOfAgreementDocumentId,
        string? attorneyAcknowledgementDocumentId,
        string? notes,
        DateOnly today,
        UserId completedBy)
    {
        if (Status != HeirTransferStatus.HeirRecorded)
            throw BusinessRuleViolation.HeirTransfer.Complete.NoHeirRecorded(Status);

        if (string.IsNullOrWhiteSpace(letterOfAgreementDocumentId) || string.IsNullOrWhiteSpace(attorneyAcknowledgementDocumentId))
            throw BusinessRuleViolation.HeirTransfer.Complete.LettersRequired();

        if (transferredOn < DiedOn)
            throw BusinessRuleViolation.HeirTransfer.Complete.TransferredOnBeforeDeath(transferredOn, DiedOn);

        if (transferredOn > today)
            throw BusinessRuleViolation.HeirTransfer.Complete.TransferredOnInFuture(transferredOn, today);

        return new HeirTransferCompleted(
            Id.Value,
            GroundLeaseId.Value,
            transferredOn,
            letterOfAgreementDocumentId.Trim(),
            attorneyAcknowledgementDocumentId.Trim(),
            Trimmed(notes),
            completedBy.Value);
    }

    /// <summary>Closes the transfer without the home passing to the heir. The lease is unchanged. Refused unless the transfer is open.</summary>
    /// <param name="reason">Why it closed.</param>
    /// <param name="notes">Anything staff want to note.</param>
    /// <param name="closedBy">The staff user closing it.</param>
    public HeirTransferClosed Close(HeirTransferCloseReason reason, string? notes, UserId closedBy)
    {
        ArgumentNullException.ThrowIfNull(reason);

        if (Status != HeirTransferStatus.Open && Status != HeirTransferStatus.HeirRecorded)
            throw BusinessRuleViolation.HeirTransfer.Close.NotOpen(Status);

        return new HeirTransferClosed(Id.Value, GroundLeaseId.Value, reason, Trimmed(notes), closedBy.Value);
    }

    private static string? Trimmed(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();

    /// <summary>Evolves a new transfer from its stream's first event.</summary>
    public static HeirTransfer Create(HeirTransferOpened e)
    {
        ArgumentNullException.ThrowIfNull(e);

        return new HeirTransfer
        {
            Id = HeirTransferId.Create(e.AggregateId),
            GroundLeaseId = GroundLeaseId.Create(e.GroundLeaseId),
            Status = HeirTransferStatus.Open,
            DiedOn = e.DiedOn,
            NoticeReceivedOn = e.NoticeReceivedOn,
        };
    }

    /// <summary>Applies <see cref="HeirRecorded"/>.</summary>
    public void Apply(HeirRecorded e)
    {
        Status = HeirTransferStatus.HeirRecorded;
        Heir = Homeowner.From(e.FullName, e.Email, e.Phone);
        Relationship = e.Relationship;
    }

    /// <summary>Applies <see cref="HeirTransferCompleted"/>.</summary>
    public void Apply(HeirTransferCompleted e)
    {
        Status = HeirTransferStatus.Completed;
        TransferredOn = e.TransferredOn;
    }

    /// <summary>Applies <see cref="HeirTransferClosed"/>.</summary>
    public void Apply(HeirTransferClosed _) => Status = HeirTransferStatus.Closed;
}
