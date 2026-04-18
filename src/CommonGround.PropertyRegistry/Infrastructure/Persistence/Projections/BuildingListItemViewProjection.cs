using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Domain.Buildings.Events;
using JasperFx.Events;
using Marten.Events.Aggregation;

namespace CommonGround.PropertyRegistry.Infrastructure.Persistence.Projections;

/// <summary>Builds <see cref="BuildingListItemView"/> from Building events.</summary>
internal sealed class BuildingListItemViewProjection : SingleStreamProjection<BuildingListItemView, string>
{
    /// <summary>Creates the initial list item from a BuildingRegistered event.</summary>
    public static BuildingListItemView Create(IEvent<BuildingRegistered> e) =>
        new()
        {
            Id = e.Data.AggregateId,
            LandParcelId = e.Data.LandParcelId,
            BuildingType = e.Data.BuildingType,
            Bedrooms = e.Data.Bedrooms,
            Bathrooms = e.Data.Bathrooms,
            SquareFootage = e.Data.SquareFootage,
            AssessedValue = e.Data.InitialAssessedValue,
            Status = "Available",
        };

    /// <summary>Applies a status change.</summary>
    public static void Apply(BuildingListItemView view, BuildingStatusChanged e)
    {
        view.Status = e.NewStatus;
    }

    /// <summary>Applies an assessment update to the assessed value.</summary>
    public static void Apply(BuildingListItemView view, BuildingAssessed e)
    {
        view.AssessedValue = e.AssessedValue;
    }
}