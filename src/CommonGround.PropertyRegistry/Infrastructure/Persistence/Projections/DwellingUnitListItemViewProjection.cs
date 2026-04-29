using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Domain.LeasableUnits.Events;
using JasperFx.Events;
using Marten.Events.Aggregation;

namespace CommonGround.PropertyRegistry.Infrastructure.Persistence.Projections;

/// <summary>Builds <see cref="DwellingUnitListItemView"/> from DwellingUnit events.</summary>
internal sealed class DwellingUnitListItemViewProjection : SingleStreamProjection<DwellingUnitListItemView, string>
{
    /// <summary>Creates the initial list item from a DwellingUnitRegistered event.</summary>
    public static DwellingUnitListItemView Create(IEvent<DwellingUnitRegistered> e) =>
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
    public static void Apply(DwellingUnitListItemView view, LeasableUnitAssessed e)
    {
        view.AssessedValue = e.AssessedValue;
    }

    /// <summary>Applies a ground lease execution — transitions to PendingOccupancy.</summary>
    public static void Apply(DwellingUnitListItemView view, GroundLeaseExecuted _)
    {
        view.Status = "PendingOccupancy";
    }

    /// <summary>Applies a move-in confirmation — transitions to Occupied.</summary>
    public static void Apply(DwellingUnitListItemView view, MoveInConfirmed _)
    {
        view.Status = "Occupied";
    }

    /// <summary>Applies a resale initiation — transitions to Resale.</summary>
    public static void Apply(DwellingUnitListItemView view, ResaleInitiated _)
    {
        view.Status = "Resale";
    }

    /// <summary>Applies a resale closing completion — transitions to PendingOccupancy.</summary>
    public static void Apply(DwellingUnitListItemView view, ResaleClosingCompleted _)
    {
        view.Status = "PendingOccupancy";
    }

    /// <summary>Applies an occupancy cancellation — transitions to Available.</summary>
    public static void Apply(DwellingUnitListItemView view, OccupancyCancelled _)
    {
        view.Status = "Available";
    }

    /// <summary>Applies a resale listing reopening — transitions to Available.</summary>
    public static void Apply(DwellingUnitListItemView view, ResaleListingReopened _)
    {
        view.Status = "Available";
    }

    /// <summary>Applies a resale abandoned for redevelopment — transitions to Available.</summary>
    public static void Apply(DwellingUnitListItemView view, ResaleAbandonedForRedevelopment _)
    {
        view.Status = "Available";
    }
}
