using System.Globalization;
using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;
using JasperFx.Events;
using Marten.Events.Aggregation;

namespace CommonGround.Homeownership.Infrastructure.Persistence.Projections;

/// <summary>Builds <see cref="PurchaseWorkflowView"/> from PurchaseWorkflow events.</summary>
internal sealed class PurchaseWorkflowViewProjection : SingleStreamProjection<PurchaseWorkflowView, string>
{
    public static PurchaseWorkflowView Create(IEvent<PurchaseWorkflowInitiated> e) =>
        new()
        {
            Id = e.Data.AggregateId,
            GroundLeaseTemplateId = e.Data.GroundLeaseTemplateId,
            LandParcelId = e.Data.LandParcelId,
            HomeId = e.Data.HomeId,
            ApplicationId = e.Data.ApplicationId,
            HouseholdId = e.Data.HouseholdId,
            Phase = "Initiated",
            InitiatedBy = e.Data.InitiatedBy,
            BuyerFullName = e.Data.BuyerFullName,
            BuyerEmail = e.Data.BuyerEmail,
            BuyerPhone = e.Data.BuyerPhone,
            PurchasePrice = e.Data.PurchasePrice,
            CreatedAt = e.Data.OccurredAt,
            DeclaredAnnualIncome = e.Data.DeclaredAnnualIncome?.Amount,
            DeclaredHouseholdSize = e.Data.DeclaredHouseholdSize,
            IncomeLimitPercentOfAmi = e.Data.IncomeLimitPercentOfAmi,
            IncomeLimitsEffectiveFrom = e.Data.IncomeLimitsEffectiveFrom,
            IncomeLimitLevels = e.Data.IncomeLimitLevels?
                .Select(level => new IncomeLimitLevelEntryView
                {
                    PercentOfAmi = level.PercentOfAmi,
                    Limits = [.. level.Limits.Select(limit => limit.Amount)],
                })
                .ToList(),
        };

    public static void Apply(PurchaseWorkflowView view, BuyerEligibilityVerified e)
    {
        view.Phase = "InProgress";
        view.EligibilityDetails = new EligibilityDetailsEntry
        {
            VerifiedDate = e.VerifiedDate,
            Notes = e.EligibilityNotes,
            VerifiedAnnualIncome = e.VerifiedAnnualIncome.Amount,
            VerifiedHouseholdSize = e.VerifiedHouseholdSize,
            IncomeLimit = e.IncomeLimit?.Amount,
            IncomeBandOverPercentOfAmi = e.IncomeBandOverPercentOfAmi,
            IncomeBandUpToPercentOfAmi = e.IncomeBandUpToPercentOfAmi,
            IncomeDocumentIds = [.. e.IncomeDocumentIds],
        };
    }

    public static void Apply(PurchaseWorkflowView view, IEvent<BuyerEligibilityCorrected> e)
    {
        var was = view.EligibilityDetails!;
        var now = new EligibilityDetailsEntry
        {
            VerifiedDate = e.Data.VerifiedDate,
            Notes = e.Data.EligibilityNotes,
            VerifiedAnnualIncome = e.Data.VerifiedAnnualIncome.Amount,
            VerifiedHouseholdSize = e.Data.VerifiedHouseholdSize,
            IncomeLimit = e.Data.IncomeLimit?.Amount,
            IncomeBandOverPercentOfAmi = e.Data.IncomeBandOverPercentOfAmi,
            IncomeBandUpToPercentOfAmi = e.Data.IncomeBandUpToPercentOfAmi,
            IncomeDocumentIds = [.. e.Data.IncomeDocumentIds],
        };

        var changes = new List<PurchaseCorrectionChange>();
        AddChange(changes, "VerifiedDate", "Date", was.VerifiedDate, now.VerifiedDate);
        AddChange(changes, "VerifiedAnnualIncome", "Money", was.VerifiedAnnualIncome, now.VerifiedAnnualIncome);
        AddChange(changes, "VerifiedHouseholdSize", "Number", was.VerifiedHouseholdSize, now.VerifiedHouseholdSize);
        AddChange(changes, "Notes", "Text", was.Notes, now.Notes);
        if (!was.IncomeDocumentIds.SequenceEqual(now.IncomeDocumentIds))
        {
            changes.Add(new PurchaseCorrectionChange
            {
                Field = "IncomeDocuments",
                Kind = "Documents",
                Was = string.Join(',', was.IncomeDocumentIds),
                Now = string.Join(',', now.IncomeDocumentIds),
            });
        }

        view.EligibilityDetails = now;
        AddCorrection(view, "Eligibility", e.Data.OccurredAt, e.Data.CorrectedBy, e.Data.Reason, changes);
    }

    public static void Apply(PurchaseWorkflowView view, IEvent<AppraisalCorrected> e)
    {
        var was = view.AppraisalDetails!;
        var now = new AppraisalDetailsEntry
        {
            AppraisedValue = e.Data.AppraisedValue,
            AppraisalDate = e.Data.AppraisalDate,
            AppraiserName = e.Data.AppraiserName,
            Notes = e.Data.AppraisalNotes,
        };

        var changes = new List<PurchaseCorrectionChange>();
        AddChange(changes, "AppraisedValue", "Money", was.AppraisedValue, now.AppraisedValue);
        AddChange(changes, "AppraisalDate", "Date", was.AppraisalDate, now.AppraisalDate);
        AddChange(changes, "AppraiserName", "Text", was.AppraiserName, now.AppraiserName);
        AddChange(changes, "Notes", "Text", was.Notes, now.Notes);

        view.AppraisalDetails = now;
        AddCorrection(view, "Appraisal", e.Data.OccurredAt, e.Data.CorrectedBy, e.Data.Reason, changes);
    }

    public static void Apply(PurchaseWorkflowView view, IEvent<FinancingCorrected> e)
    {
        var was = view.FinancingDetails!;
        var now = new FinancingDetailsEntry
        {
            LenderName = e.Data.LenderName,
            CommittedAmount = e.Data.CommittedAmount,
            PreapprovalLetterDate = e.Data.PreapprovalLetterDate,
        };

        var changes = new List<PurchaseCorrectionChange>();
        AddChange(changes, "LenderName", "Text", was.LenderName, now.LenderName);
        AddChange(changes, "CommittedAmount", "Money", was.CommittedAmount, now.CommittedAmount);
        AddChange(changes, "PreapprovalLetterDate", "Date", was.PreapprovalLetterDate, now.PreapprovalLetterDate);

        view.FinancingDetails = now;
        AddCorrection(view, "Financing", e.Data.OccurredAt, e.Data.CorrectedBy, e.Data.Reason, changes);
    }

    private static void AddCorrection(
        PurchaseWorkflowView view, string track, DateTimeOffset at, string by, string reason,
        List<PurchaseCorrectionChange> changes) =>
        view.Corrections.Add(new PurchaseCorrectionEntry
        {
            Track = track,
            CorrectedAt = at,
            CorrectedBy = by,
            Reason = reason,
            Changes = changes,
        });

    private static void AddChange<T>(List<PurchaseCorrectionChange> changes, string field, string kind, T was, T now)
        where T : IFormattable?
    {
        if (EqualityComparer<T>.Default.Equals(was, now)) return;
        changes.Add(new PurchaseCorrectionChange
        {
            Field = field,
            Kind = kind,
            Was = was?.ToString(null, CultureInfo.InvariantCulture),
            Now = now?.ToString(null, CultureInfo.InvariantCulture),
        });
    }

    private static void AddChange(List<PurchaseCorrectionChange> changes, string field, string kind, string? was, string? now)
    {
        if (was == now) return;
        changes.Add(new PurchaseCorrectionChange { Field = field, Kind = kind, Was = was, Now = now });
    }

    public static void Apply(PurchaseWorkflowView view, BuyerDisclosureAcknowledged e)
    {
        view.Phase = "InProgress";
        view.DisclosureDetails = new DisclosureDetailsEntry
        {
            AcknowledgedDate = e.AcknowledgedDate,
            Notes = e.DisclosureNotes,
            DocumentId = e.DocumentId,
        };
    }

    public static void Apply(PurchaseWorkflowView view, AppraisalCompleted e)
    {
        view.Phase = "InProgress";
        view.AppraisalDetails = new AppraisalDetailsEntry
        {
            AppraisedValue = e.AppraisedValue,
            AppraisalDate = e.AppraisalDate,
            AppraiserName = e.AppraiserName,
            Notes = e.AppraisalNotes,
        };
    }

    public static void Apply(PurchaseWorkflowView view, FinancingCommitted e)
    {
        view.Phase = "InProgress";
        view.FinancingDetails = new FinancingDetailsEntry
        {
            LenderName = e.LenderName,
            CommittedAmount = e.CommittedAmount,
            PreapprovalLetterDate = e.PreapprovalLetterDate,
        };
    }

    public static void Apply(PurchaseWorkflowView view, InspectionCompleted e)
    {
        view.Phase = "InProgress";
        view.InspectionResult = new InspectionResultEntry
        {
            InspectionDate = e.InspectionDate,
            InspectorName = e.InspectorName,
            Notes = e.InspectionNotes,
        };
    }

    public static void Apply(PurchaseWorkflowView view, TitleCleared e)
    {
        view.Phase = "InProgress";
        view.TitleDetails = new TitleDetailsEntry
        {
            ClearedDate = e.ClearedDate,
            TitleCompanyName = e.TitleCompanyName,
        };
    }

    public static void Apply(PurchaseWorkflowView view, LeaseExecuted e)
    {
        view.Phase = "InProgress";
        view.LeaseExecutionDetails = new LeaseExecutionDetailsEntry
        {
            SignedDate = e.SignedDate,
        };
    }

    public static void Apply(PurchaseWorkflowView view, ClosingScheduled e)
    {
        view.Phase = "ClosingScheduled";
        view.ScheduledClosingDate = e.ScheduledClosingDate;
    }

    public static void Apply(PurchaseWorkflowView view, PurchaseClosed e)
    {
        view.Phase = "Closed";
        view.ActualClosingDate = e.ActualClosingDate;
        view.ClosingNotes = e.ClosingNotes;
    }

    public static void Apply(PurchaseWorkflowView view, PurchaseFailed e)
    {
        view.Phase = "Failed";
        view.FailureReason = e.FailureReason;
    }

    public static void Apply(PurchaseWorkflowView view, PurchaseWithdrawn e)
    {
        view.Phase = "Withdrawn";
        view.WithdrawalReason = e.WithdrawalReason;
    }

    public static void Apply(PurchaseWorkflowView view, WorkflowStalled e)
    {
        view.Phase = "Stalled";
        view.StalledReason = e.StalledReason;
        view.StalledDate = e.StalledDate;
        view.ResolutionTargetDate = e.ResolutionTargetDate;
    }
}
