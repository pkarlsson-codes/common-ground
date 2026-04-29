using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Domain.LeasableUnits.Events;
using JasperFx.Events;
using Marten.Events.Aggregation;

namespace CommonGround.PropertyRegistry.Infrastructure.Persistence.Projections;

/// <summary>Builds <see cref="DwellingUnitView"/> from DwellingUnit events.</summary>
internal sealed class DwellingUnitViewProjection : SingleStreamProjection<DwellingUnitView, string>
{
    /// <summary>Creates the initial view from a DwellingUnitRegistered event.</summary>
    public static DwellingUnitView Create(IEvent<DwellingUnitRegistered> e) =>
        new()
        {
            Id = e.Data.AggregateId,
            LandParcelId = e.Data.LandParcelId,
            UnitType = e.Data.UnitType,
            Bedrooms = e.Data.Bedrooms,
            Bathrooms = e.Data.Bathrooms,
            SquareFootage = e.Data.SquareFootage,
            YearBuilt = e.Data.YearBuilt,
            AssessedValue = e.Data.InitialAssessedValue,
            Status = "Available",
            RegisteredAt = e.Data.OccurredAt,
        };

    /// <summary>Applies an assessment update.</summary>
    public static void Apply(DwellingUnitView view, LeasableUnitAssessed e)
    {
        view.AssessedValue = e.AssessedValue;
        view.LastAssessedDate = e.AssessmentDate;
        view.ConditionNotes = e.ConditionNotes;
    }

    /// <summary>Applies a ground lease execution — transitions to PendingOccupancy.</summary>
    public static void Apply(DwellingUnitView view, GroundLeaseExecuted _)
    {
        view.Status = "PendingOccupancy";
    }

    /// <summary>Applies a move-in confirmation — transitions to Occupied.</summary>
    public static void Apply(DwellingUnitView view, MoveInConfirmed _)
    {
        view.Status = "Occupied";
    }

    /// <summary>Applies a resale initiation — transitions to Resale.</summary>
    public static void Apply(DwellingUnitView view, ResaleInitiated _)
    {
        view.Status = "Resale";
    }

    /// <summary>Applies a resale closing completion — transitions to PendingOccupancy.</summary>
    public static void Apply(DwellingUnitView view, ResaleClosingCompleted _)
    {
        view.Status = "PendingOccupancy";
    }

    /// <summary>Applies an occupancy cancellation — transitions to Available.</summary>
    public static void Apply(DwellingUnitView view, OccupancyCancelled _)
    {
        view.Status = "Available";
    }

    /// <summary>Applies a resale listing reopening — transitions to Available.</summary>
    public static void Apply(DwellingUnitView view, ResaleListingReopened _)
    {
        view.Status = "Available";
    }

    /// <summary>Applies a resale abandoned for redevelopment — transitions to Available.</summary>
    public static void Apply(DwellingUnitView view, ResaleAbandonedForRedevelopment _)
    {
        view.Status = "Available";
    }
}
