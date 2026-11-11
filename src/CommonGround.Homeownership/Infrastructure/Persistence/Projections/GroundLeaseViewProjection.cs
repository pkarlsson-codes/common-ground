using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeases.Events;
using JasperFx.Events;
using Marten.Events.Aggregation;

namespace CommonGround.Homeownership.Infrastructure.Persistence.Projections;

/// <summary>Builds <see cref="GroundLeaseView"/> from GroundLease events.</summary>
internal sealed class GroundLeaseViewProjection : SingleStreamProjection<GroundLeaseView, string>
{
    public static GroundLeaseView Create(IEvent<GroundLeaseCreated> e) =>
        new()
        {
            Id = e.Data.AggregateId,
            LandParcelId = e.Data.LandParcelId,
            HomeId = e.Data.HomeId,
            LeaseStartDate = e.Data.LeaseStartDate,
            TermYears = e.Data.TermYears,
            OptionWindowDays = e.Data.OptionWindowDays,
            LandUseFee = e.Data.LandUseFee,
            RepairReserveFee = e.Data.RepairReserveFee,
            LateInterestRate = e.Data.LateInterestRate,
            InterestWaiverDays = e.Data.InterestWaiverDays,
            BasePrice = e.Data.BasePrice,
            OriginalAppraisedValue = e.Data.OriginalAppraisedValue,
            CovenantTerms = e.Data.CovenantTerms,
            Status = "Active",
            CreatedBy = e.Data.RegisteredBy,
            CreatedAt = e.Data.OccurredAt,
            FormulaKind = e.Data.FormulaKind.ToString(),
            AppreciationShare = e.Data.AppreciationShare,
            IndexType = e.Data.IndexType,
            BaselineIndexValue = e.Data.BaselineIndexValue,
            AnnualPercentage = e.Data.AnnualPercentage,
            ImprovementsCap = e.Data.ImprovementsCap,
            HomeownerFullName = e.Data.HomeownerFullName,
            HomeownerEmail = e.Data.HomeownerEmail,
            HomeownerPhone = e.Data.HomeownerPhone,
        };

    public static GroundLeaseView Create(IEvent<GroundLeaseCreatedFromPurchase> e) =>
        new()
        {
            Id = e.Data.AggregateId,
            GroundLeaseTemplateId = e.Data.GroundLeaseTemplateId,
            PurchaseWorkflowId = e.Data.PurchaseWorkflowId,
            LandParcelId = e.Data.LandParcelId,
            HomeId = e.Data.HomeId,
            LeaseStartDate = e.Data.LeaseStartDate,
            TermYears = e.Data.TermYears,
            OptionWindowDays = e.Data.OptionWindowDays,
            LandUseFee = e.Data.LandUseFee,
            RepairReserveFee = e.Data.RepairReserveFee,
            LateInterestRate = e.Data.LateInterestRate,
            InterestWaiverDays = e.Data.InterestWaiverDays,
            BasePrice = e.Data.BasePrice,
            OriginalAppraisedValue = e.Data.OriginalAppraisedValue,
            CovenantTerms = e.Data.CovenantTerms,
            Status = "Active",
            CreatedBy = e.Data.RegisteredBy,
            CreatedAt = e.Data.OccurredAt,
            FormulaKind = e.Data.FormulaKind.ToString(),
            AppreciationShare = e.Data.AppreciationShare,
            IndexType = e.Data.IndexType,
            BaselineIndexValue = e.Data.BaselineIndexValue,
            AnnualPercentage = e.Data.AnnualPercentage,
            ImprovementsCap = e.Data.ImprovementsCap,
            HomeownerFullName = e.Data.BuyerFullName,
            HomeownerEmail = e.Data.BuyerEmail,
            HomeownerPhone = e.Data.BuyerPhone,
        };

    public static void Apply(GroundLeaseView view, StewardshipCheckInRecorded e)
    {
        view.StewardshipCheckIns.Add(new StewardshipCheckInEntry
        {
            CheckInDate = e.CheckInDate,
            Notes = e.Notes,
            RecordedBy = e.RecordedBy,
            OccurredAt = e.OccurredAt,
        });
    }

    public static void Apply(GroundLeaseView view, GroundLeaseTerminated e)
    {
        view.Status = "Inactive";
        view.EndedOn = e.TerminatedDate;
    }

    public static void Apply(GroundLeaseView view, LeaseFeePaymentRecorded e) =>
        view.LeaseFeePayments.Add(new LeaseFeePaymentEntry
        {
            PaymentId = e.PaymentId,
            ReceivedDate = e.ReceivedDate,
            Amount = e.Amount,
            Method = e.Method.DisplayName,
            Reference = e.Reference,
            RecordedBy = e.RecordedBy,
            RecordedAt = e.OccurredAt,
        });

    public static void Apply(GroundLeaseView view, LeaseFeeNoticeRecorded e) =>
        view.LeaseFeeNotices.Add(new LeaseFeeNoticeEntry
        {
            NoticeId = e.NoticeId,
            Level = e.Level.DisplayName,
            SentOn = e.SentOn,
            Delivery = e.Delivery.DisplayName,
            BalanceDue = e.BalanceDue,
            OldestUnpaidDueDate = e.OldestUnpaidDueDate,
            CureDays = e.CureDays,
            CureExtensionDays = e.CureExtensionDays,
            RecordedBy = e.RecordedBy,
            RecordedAt = e.OccurredAt,
        });

    public static void Apply(GroundLeaseView view, LeaseFeeInterestWaived e) =>
        view.LeaseFeeInterestWaivers.Add(new LeaseFeeInterestWaiverEntry
        {
            WaiverId = e.WaiverId,
            WaivedOn = e.WaivedOn,
            Amount = e.Amount,
            Reason = e.Reason,
            WaivedBy = e.WaivedBy,
        });

    public static void Apply(GroundLeaseView view, LeaseFeePaymentVoided e)
    {
        var payment = view.LeaseFeePayments.Find(p => p.PaymentId == e.PaymentId);
        if (payment is null)
            return;

        payment.VoidReason = e.Reason;
        payment.VoidedBy = e.VoidedBy;
        payment.VoidedAt = e.OccurredAt;
    }
}
