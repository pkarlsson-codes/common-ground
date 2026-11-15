using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.HeirTransfers;
using CommonGround.Homeownership.Domain.HeirTransfers.Events;
using Marten.Events.Aggregation;

namespace CommonGround.Homeownership.Infrastructure.Persistence.Projections;

/// <summary>Builds <see cref="HeirTransferView"/> from HeirTransfer events.</summary>
internal sealed class HeirTransferViewProjection : SingleStreamProjection<HeirTransferView, string>
{
    public static HeirTransferView Create(HeirTransferOpened e) =>
        new()
        {
            Id = e.AggregateId,
            GroundLeaseId = e.GroundLeaseId,
            HomeId = e.HomeId,
            DeceasedHomeownerFullName = e.DeceasedHomeownerFullName,
            Status = HeirTransferStatus.Open.DisplayName,
            DiedOn = e.DiedOn,
            NoticeReceivedOn = e.NoticeReceivedOn,
            NoticeDueOn = e.DiedOn.AddDays(HeirTransfer.NoticeDays),
            NoticeWasLate = e.NoticeReceivedOn > e.DiedOn.AddDays(HeirTransfer.NoticeDays),
            NotifiedBy = e.NotifiedBy,
            NotifiedByContact = e.NotifiedByContact,
            Notes = e.Notes,
            OpenedAt = e.OccurredAt,
        };

    public static void Apply(HeirTransferView view, HeirRecorded e)
    {
        view.Status = HeirTransferStatus.HeirRecorded.DisplayName;
        view.HeirFullName = e.FullName;
        view.HeirEmail = e.Email;
        view.HeirPhone = e.Phone;
        view.Relationship = e.Relationship.DisplayName;
        view.HeirMovedInOn = e.MovedInOn;
        view.HeirVerifiedAnnualIncome = e.VerifiedAnnualIncome?.Amount;
        view.HeirVerifiedHouseholdSize = e.VerifiedHouseholdSize;
        view.IncomeDocumentIds = [.. e.IncomeDocumentIds];
        view.IncomeLimit = e.IncomeLimit?.Amount;
        view.IncomeLimitPercentOfAmi = e.IncomeLimitPercentOfAmi;
        view.IncomeLimitsEffectiveFrom = e.IncomeLimitsEffectiveFrom;
    }

    public static void Apply(HeirTransferView view, HeirTransferCompleted e)
    {
        view.Status = HeirTransferStatus.Completed.DisplayName;
        view.TransferredOn = e.TransferredOn;
        view.LetterOfAgreementDocumentId = e.LetterOfAgreementDocumentId;
        view.AttorneyAcknowledgementDocumentId = e.AttorneyAcknowledgementDocumentId;
        view.CompletionNotes = e.Notes;
    }

    public static void Apply(HeirTransferView view, HeirTransferClosed e)
    {
        view.Status = HeirTransferStatus.Closed.DisplayName;
        view.ClosedReason = e.Reason.DisplayName;
        view.ClosedNotes = e.Notes;
        view.ClosedAt = e.OccurredAt;
    }
}
