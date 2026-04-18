using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Domain.Buildings.Events;
using JasperFx.Events;
using Marten.Events.Aggregation;

namespace CommonGround.PropertyRegistry.Infrastructure.Persistence.Projections;

/// <summary>Builds <see cref="BuildingView"/> from Building events.</summary>
internal sealed class BuildingViewProjection : SingleStreamProjection<BuildingView, string>
{
    /// <summary>Creates the initial view from a BuildingRegistered event.</summary>
    public static BuildingView Create(IEvent<BuildingRegistered> e) =>
        new()
        {
            Id = e.Data.AggregateId,
            LandParcelId = e.Data.LandParcelId,
            BuildingType = e.Data.BuildingType,
            Bedrooms = e.Data.Bedrooms,
            Bathrooms = e.Data.Bathrooms,
            SquareFootage = e.Data.SquareFootage,
            YearBuilt = e.Data.YearBuilt,
            AssessedValue = e.Data.InitialAssessedValue,
            Status = "Available",
            RegisteredAt = e.Data.OccurredAt,
        };

    /// <summary>Applies a status change.</summary>
    public static void Apply(BuildingView view, BuildingStatusChanged e)
    {
        view.Status = e.NewStatus;
    }

    /// <summary>Applies an assessment update.</summary>
    public static void Apply(BuildingView view, BuildingAssessed e)
    {
        view.AssessedValue = e.AssessedValue;
        view.LastAssessedDate = e.AssessmentDate;
        view.ConditionNotes = e.ConditionNotes;
    }
}