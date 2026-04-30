using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeases.Events;
using JasperFx.Events;
using Marten.Events.Aggregation;

namespace CommonGround.Homeownership.Infrastructure.Persistence.Projections;

/// <summary>Builds <see cref="GroundLeaseListItemView"/> from GroundLease events.</summary>
internal sealed class GroundLeaseListItemViewProjection : SingleStreamProjection<GroundLeaseListItemView, string>
{
    public static GroundLeaseListItemView Create(IEvent<GroundLeaseCreated> e) =>
        new()
        {
            Id = e.Data.AggregateId,
            LandParcelId = e.Data.LandParcelId,
            HomeId = e.Data.BuildingId,
            LeaseStartDate = e.Data.LeaseStartDate,
            TermYears = e.Data.TermYears,
            Status = "Active",
            CreatedAt = e.Data.OccurredAt,
        };

    public static void Apply(GroundLeaseListItemView view, HomeownerOnboarded e)
    {
        view.HomeownerFullName = e.FullName;
        view.HomeownerEmail = e.Email;
    }

    public static void Apply(GroundLeaseListItemView view, HomeownerChanged e)
    {
        view.HomeownerFullName = e.NewFullName;
        view.HomeownerEmail = e.NewEmail;
    }
}
