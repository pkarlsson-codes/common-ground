using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.Resales;
using CommonGround.Homeownership.Domain.Resales.Events;
using Marten.Events.Aggregation;

namespace CommonGround.Homeownership.Infrastructure.Persistence.Projections;

/// <summary>Builds <see cref="ResaleView"/> from Resale events.</summary>
internal sealed class ResaleViewProjection : SingleStreamProjection<ResaleView, string>
{
    public static ResaleView Create(IntentToSellRecorded e) =>
        new()
        {
            Id = e.AggregateId,
            GroundLeaseId = e.GroundLeaseId,
            LandParcelId = e.LandParcelId,
            HomeId = e.HomeId,
            HomeownerFullName = e.HomeownerFullName,
            Status = ResaleStatus.Open.DisplayName,
            NoticeDate = e.NoticeDate,
            NoticeChannel = e.Channel.DisplayName,
            Notes = e.Notes,
            OptionWindowEndsOn = e.OptionWindowEndsOn,
            RecordedBy = e.RecordedBy,
            RecordedAt = e.OccurredAt,
        };

    public static void Apply(ResaleView view, ResaleWithdrawn e)
    {
        view.Status = ResaleStatus.Withdrawn.DisplayName;
        view.WithdrawnDate = e.WithdrawnDate;
        view.WithdrawnReason = e.Reason;
    }

    public static void Apply(ResaleView view, ResaleAppraisalRecorded e)
    {
        view.AppraisedValue = e.AppraisedValue.Amount;
        view.AppraisalDate = e.AppraisalDate;
        view.AppraiserName = e.AppraiserName;
        view.ApprovedImprovements = e.ApprovedImprovements?.Amount;
        view.CurrentIndexValue = e.CurrentIndexValue;
        view.FormulaPrice = e.FormulaPrice.Amount;
        view.PurchaseOptionPrice = e.PurchaseOptionPrice.Amount;
    }

    public static void Apply(ResaleView view, PurchaseOptionExercised e)
    {
        view.OptionExercisedDate = e.ExercisedDate;
        view.OptionExercisedNotes = e.Notes;
    }

    public static void Apply(ResaleView view, BuybackClosed e)
    {
        view.Status = ResaleStatus.Closed.DisplayName;
        view.ClosingDate = e.ClosingDate;
        view.PricePaid = e.PricePaid.Amount;
        view.ClosingNotes = e.Notes;
        view.OfferedForSale = e.OfferedForSale;
        view.HoldReason = e.HoldReason;
        view.FeesPaidFromProceeds = e.FeesPaidFromProceeds?.Amount;
    }

    public static void Apply(ResaleView view, PayoffStatementRecorded e) =>
        view.PayoffStatements.Add(new PayoffStatementPrint(e.OccurredAt, e.RecordedBy, e.GoodThrough, e.TotalOwed.Amount));
}
