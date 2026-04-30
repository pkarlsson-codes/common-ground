using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Domain.LeasableUnits.Events;
using JasperFx.Events;
using Marten.Events.Aggregation;

namespace CommonGround.PropertyRegistry.Infrastructure.Persistence.Projections;

/// <summary>Builds <see cref="HomeListItemView"/> from Home events.</summary>
internal sealed class HomeListItemViewProjection : SingleStreamProjection<HomeListItemView, string>
{
    /// <summary>Creates the initial list item from a HomeRegistered event.</summary>
    public static HomeListItemView Create(IEvent<HomeRegistered> e) =>
        new()
        {
            Id = e.Data.AggregateId,
            LandParcelId = e.Data.LandParcelId,
            UnitType = e.Data.UnitType,
            Bedrooms = e.Data.Bedrooms,
            Bathrooms = e.Data.Bathrooms,
            SquareFootage = e.Data.SquareFootage,
            AssessedValue = e.Data.InitialAssessedValue,
            Status = "Available",
        };

    /// <summary>Applies an assessment update to the assessed value.</summary>
    public static void Apply(HomeListItemView view, LeasableUnitAssessed e)
    {
        view.AssessedValue = e.AssessedValue;
    }

    /// <summary>Applies a ground lease execution — transitions to PendingOccupancy.</summary>
    public static void Apply(HomeListItemView view, GroundLeaseExecuted _)
    {
        view.Status = "PendingOccupancy";
    }

    /// <summary>Applies a move-in confirmation — transitions to Occupied.</summary>
    public static void Apply(HomeListItemView view, MoveInConfirmed _)
    {
        view.Status = "Occupied";
    }

    /// <summary>Applies a resale initiation — transitions to Resale.</summary>
    public static void Apply(HomeListItemView view, ResaleInitiated _)
    {
        view.Status = "Resale";
    }

    /// <summary>Applies a resale closing completion — transitions to PendingOccupancy.</summary>
    public static void Apply(HomeListItemView view, ResaleClosingCompleted _)
    {
        view.Status = "PendingOccupancy";
    }

    /// <summary>Applies an occupancy cancellation — transitions to Available.</summary>
    public static void Apply(HomeListItemView view, OccupancyCancelled _)
    {
        view.Status = "Available";
    }

    /// <summary>Applies a resale listing reopening — transitions to Available.</summary>
    public static void Apply(HomeListItemView view, ResaleListingReopened _)
    {
        view.Status = "Available";
    }

    /// <summary>Applies a resale abandoned for redevelopment — transitions to Available.</summary>
    public static void Apply(HomeListItemView view, ResaleAbandonedForRedevelopment _)
    {
        view.Status = "Available";
    }
}
