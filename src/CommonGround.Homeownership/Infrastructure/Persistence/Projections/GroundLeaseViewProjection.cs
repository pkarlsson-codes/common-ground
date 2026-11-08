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

    public static void Apply(GroundLeaseView view, GroundLeaseTerminated _) => view.Status = "Inactive";
}
