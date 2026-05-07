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
        };

    public static void Apply(GroundLeaseView view, HomeownerOnboarded e)
    {
        view.HomeownerFullName = e.FullName;
        view.HomeownerEmail = e.Email;
        view.HomeownerPhone = e.Phone;
    }

    public static void Apply(GroundLeaseView view, HomeownerChanged e)
    {
        view.HomeownerFullName = e.NewFullName;
        view.HomeownerEmail = e.NewEmail;
        view.HomeownerPhone = e.NewPhone;
    }

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
}
