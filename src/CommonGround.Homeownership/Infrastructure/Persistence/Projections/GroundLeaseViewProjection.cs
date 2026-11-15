using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeases.Events;
using CommonGround.Homeownership.Domain.GroundLeases.Mortgages;
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
            LandUseFee = e.Data.LandUseFee.Amount,
            RepairReserveFee = e.Data.RepairReserveFee.Amount,
            LateInterestRate = e.Data.LateInterestRate,
            InterestWaiverDays = e.Data.InterestWaiverDays,
            MaxMortgageDebtPercent = e.Data.MaxMortgageDebtPercent,
            InsuranceRequirements =
            [
                new InsuranceRequirementsEntry
                {
                    EffectiveFrom = e.Data.LeaseStartDate,
                    ReplacementValue = e.Data.RequiredReplacementValue?.Amount,
                    LiabilityPerOccurrence = e.Data.RequiredLiabilityPerOccurrence?.Amount,
                    SetBy = e.Data.RegisteredBy,
                    SetAt = e.Data.OccurredAt,
                },
            ],
            FeesTrackedFrom = e.Data.FeesTrackedFrom,
            OpeningBalance = e.Data.OpeningBalance.Amount,
            BasePrice = e.Data.BasePrice.Amount,
            OriginalAppraisedValue = e.Data.OriginalAppraisedValue?.Amount,
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
            PermittedMortgages = e.Data.PurchaseMortgageId is { } mortgageId
                ?
                [
                    new PermittedMortgageEntry
                    {
                        MortgageId = mortgageId,
                        Lender = e.Data.PurchaseLender!,
                        OriginalPrincipal = e.Data.PurchaseLoanPrincipal!.Amount,
                        PermittedOn = e.Data.LeaseStartDate,
                        Source = MortgagePermissionSource.LeaseSigning.DisplayName,
                    },
                ]
                : [],
            Id = e.Data.AggregateId,
            GroundLeaseTemplateId = e.Data.GroundLeaseTemplateId,
            PurchaseWorkflowId = e.Data.PurchaseWorkflowId,
            LandParcelId = e.Data.LandParcelId,
            HomeId = e.Data.HomeId,
            LeaseStartDate = e.Data.LeaseStartDate,
            TermYears = e.Data.TermYears,
            OptionWindowDays = e.Data.OptionWindowDays,
            LandUseFee = e.Data.LandUseFee.Amount,
            RepairReserveFee = e.Data.RepairReserveFee.Amount,
            LateInterestRate = e.Data.LateInterestRate,
            InterestWaiverDays = e.Data.InterestWaiverDays,
            MaxMortgageDebtPercent = e.Data.MaxMortgageDebtPercent,
            InsuranceRequirements =
            [
                new InsuranceRequirementsEntry
                {
                    EffectiveFrom = e.Data.LeaseStartDate,
                    LiabilityPerOccurrence = e.Data.RequiredLiabilityPerOccurrence?.Amount,
                    SetBy = e.Data.RegisteredBy,
                    SetAt = e.Data.OccurredAt,
                },
            ],
            FeesTrackedFrom = e.Data.LeaseStartDate,
            BasePrice = e.Data.BasePrice.Amount,
            OriginalAppraisedValue = e.Data.OriginalAppraisedValue?.Amount,
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

    public static void Apply(GroundLeaseView view, InsuranceRequirementsSet e) =>
        view.InsuranceRequirements.Add(new InsuranceRequirementsEntry
        {
            EffectiveFrom = e.EffectiveFrom,
            ReplacementValue = e.ReplacementValue?.Amount,
            LiabilityPerOccurrence = e.LiabilityPerOccurrence?.Amount,
            SetBy = e.SetBy,
            SetAt = e.OccurredAt,
        });

    public static void Apply(GroundLeaseView view, InsuranceCertificateRecorded e) =>
        view.InsuranceCertificates.Add(new InsuranceCertificateEntry
        {
            CertificateId = e.CertificateId,
            Insurer = e.Insurer,
            PolicyNumber = e.PolicyNumber,
            PolicyStart = e.PolicyStart,
            PolicyEnd = e.PolicyEnd,
            DwellingCoverage = e.DwellingCoverage.Amount,
            LiabilityPerOccurrence = e.LiabilityPerOccurrence.Amount,
            TrustNamedAdditionalInsured = e.TrustNamedAdditionalInsured,
            ReceivedOn = e.ReceivedOn,
            Notes = e.Notes,
            DocumentId = e.DocumentId,
            RecordedBy = e.RecordedBy,
            RecordedAt = e.OccurredAt,
        });

    public static void Apply(GroundLeaseView view, InsuranceCertificateRemoved e)
    {
        var certificate = view.InsuranceCertificates.Find(c => c.CertificateId == e.CertificateId);
        if (certificate is null)
            return;

        certificate.RemovedReason = e.Reason;
        certificate.RemovedBy = e.RemovedBy;
        certificate.RemovedAt = e.OccurredAt;
    }

    public static void Apply(GroundLeaseView view, HomeownerPassedToHeir e)
    {
        if (e.PreviousFullName is { } previousName && e.PreviousEmail is { } previousEmail)
        {
            view.PreviousHomeowners.Add(new PreviousHomeownerEntry
            {
                FullName = previousName,
                Email = previousEmail,
                Phone = e.PreviousPhone,
                TransferredOn = e.TransferredOn,
                HeirTransferId = e.HeirTransferId,
            });
        }

        view.HomeownerFullName = e.HomeownerFullName;
        view.HomeownerEmail = e.HomeownerEmail;
        view.HomeownerPhone = e.HomeownerPhone;
    }

    public static void Apply(GroundLeaseView view, ResaleStarted e) => view.OpenResaleId = e.ResaleId;

    public static void Apply(GroundLeaseView view, ResaleClosed e) => view.OpenResaleId = null;

    public static void Apply(GroundLeaseView view, ResalePriceStatementRecorded e) =>
        view.ResalePriceStatements.Add(new ResalePriceStatementEntry
        {
            StatementDate = e.StatementDate,
            Delivery = e.Delivery.DisplayName,
            YearsOwned = e.YearsOwned,
            MarketValue = e.MarketValue?.Amount,
            MarketValueSource = e.MarketValueSource,
            IndexValue = e.IndexValue,
            ImprovementsCredit = e.ImprovementsCredit?.Amount,
            Price = e.Price.Amount,
            RecordedBy = e.RecordedBy,
            RecordedAt = e.OccurredAt,
        });

    public static void Apply(GroundLeaseView view, LeaseFeePaymentRecorded e) =>
        view.LeaseFeePayments.Add(new LeaseFeePaymentEntry
        {
            PaymentId = e.PaymentId,
            ReceivedDate = e.ReceivedDate,
            Amount = e.Amount.Amount,
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
            BalanceDue = e.BalanceDue.Amount,
            OldestUnpaidDueDate = e.OldestUnpaidDueDate,
            CureDays = e.CureDays,
            CureExtensionDays = e.CureExtensionDays,
            CopiedTo = e.CopiedTo?
                .Select(c => new MortgageeCopyEntry { MortgageId = c.MortgageId, Lender = c.Lender, NoticeAddress = c.NoticeAddress })
                .ToList(),
            RecordedBy = e.RecordedBy,
            RecordedAt = e.OccurredAt,
        });

    public static void Apply(GroundLeaseView view, LeaseFeeInterestWaived e) =>
        view.LeaseFeeInterestWaivers.Add(new LeaseFeeInterestWaiverEntry
        {
            WaiverId = e.WaiverId,
            WaivedOn = e.WaivedOn,
            Amount = e.Amount.Amount,
            Reason = e.Reason,
            WaivedBy = e.WaivedBy,
        });

    public static void Apply(GroundLeaseView view, LeaseFeeTrackingCorrected e)
    {
        view.FeesTrackedFrom = e.FeesTrackedFrom;
        view.OpeningBalance = e.OpeningBalance.Amount;
    }

    public static void Apply(GroundLeaseView view, LeaseFeePaymentVoided e)
    {
        var payment = view.LeaseFeePayments.Find(p => p.PaymentId == e.PaymentId);
        if (payment is null)
            return;

        payment.VoidReason = e.Reason;
        payment.VoidedBy = e.VoidedBy;
        payment.VoidedAt = e.OccurredAt;
    }

    public static void Apply(GroundLeaseView view, PermittedMortgageRecorded e) =>
        view.PermittedMortgages.Add(new PermittedMortgageEntry
        {
            MortgageId = e.MortgageId,
            Lender = e.Lender,
            NoticeAddress = e.NoticeAddress,
            OriginalPrincipal = e.OriginalPrincipal.Amount,
            PermittedOn = e.PermittedOn,
            Source = e.Source.DisplayName,
        });

    public static void Apply(GroundLeaseView view, PermittedMortgageEdited e)
    {
        var mortgage = view.PermittedMortgages.Find(m => m.MortgageId == e.MortgageId);
        if (mortgage is null)
            return;

        mortgage.Lender = e.Lender;
        mortgage.NoticeAddress = e.NoticeAddress;
    }

    public static void Apply(GroundLeaseView view, PermittedMortgagePaidOff e)
    {
        var mortgage = view.PermittedMortgages.Find(m => m.MortgageId == e.MortgageId);
        if (mortgage is null)
            return;

        mortgage.PaidOffOn = e.PaidOffOn;
    }
}
