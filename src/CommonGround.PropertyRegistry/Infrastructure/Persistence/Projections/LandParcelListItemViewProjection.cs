using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Domain.LandParcels.Events;
using JasperFx.Events;
using Marten.Events.Aggregation;

namespace CommonGround.PropertyRegistry.Infrastructure.Persistence.Projections;

/// <summary>Builds <see cref="LandParcelListItemView"/> from LandParcel events.</summary>
internal sealed class LandParcelListItemViewProjection : SingleStreamProjection<LandParcelListItemView, string>
{
    /// <summary>Creates the initial list item from a LandParcelRegistered event.</summary>
    public static LandParcelListItemView Create(IEvent<LandParcelRegistered> e) =>
        new()
        {
            Id = e.Data.AggregateId,
            LegalParcelNumber = e.Data.LegalParcelNumber,
            Address = e.Data.Address,
            AreaAcres = e.Data.AreaAcres,
            Status = "Available",
            RegisteredAt = e.Data.OccurredAt,
        };

    public static void Apply(LandParcelListItemView view, ParcelEnteredPredevelopment e)
    {
        view.Status = "Predevelopment";
    }

    public static void Apply(LandParcelListItemView view, ParcelConstructionStarted e)
    {
        view.Status = "Under Construction";
    }

    public static void Apply(LandParcelListItemView view, ParcelMarkedReadyForOccupancy e)
    {
        view.Status = "Ready for Occupancy";
    }

    public static void Apply(LandParcelListItemView view, ParcelOccupied e)
    {
        view.Status = "Occupied";
    }

    public static void Apply(LandParcelListItemView view, ParcelEnteredTransitioning e)
    {
        view.Status = "Transitioning";
    }

    public static void Apply(LandParcelListItemView view, ParcelReturnedToAvailable e)
    {
        view.Status = "Available";
    }

    public static void Apply(LandParcelListItemView view, ParcelDecommissioned e)
    {
        view.Status = "Decommissioned";
    }

    public static void Apply(LandParcelListItemView view, ParcelRecommissioned e)
    {
        view.Status = "Available";
    }
}