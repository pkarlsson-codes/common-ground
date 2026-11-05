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
            HomeId = e.Data.HomeId,
            LeaseStartDate = e.Data.LeaseStartDate,
            TermYears = e.Data.TermYears,
            Status = "Active",
            FormulaKind = e.Data.FormulaKind.ToString(),
            CreatedAt = e.Data.OccurredAt,
            HomeownerFullName = e.Data.HomeownerFullName,
            HomeownerEmail = e.Data.HomeownerEmail,
        };

    public static GroundLeaseListItemView Create(IEvent<GroundLeaseCreatedFromPurchase> e) =>
        new()
        {
            Id = e.Data.AggregateId,
            LandParcelId = e.Data.LandParcelId,
            HomeId = e.Data.HomeId,
            PurchaseWorkflowId = e.Data.PurchaseWorkflowId,
            LeaseStartDate = e.Data.LeaseStartDate,
            TermYears = e.Data.TermYears,
            Status = "Active",
            FormulaKind = e.Data.FormulaKind.ToString(),
            CreatedAt = e.Data.OccurredAt,
            HomeownerFullName = e.Data.BuyerFullName,
            HomeownerEmail = e.Data.BuyerEmail,
        };

    public static void Apply(GroundLeaseListItemView view, GroundLeaseTerminated _) => view.Status = "Inactive";
}
