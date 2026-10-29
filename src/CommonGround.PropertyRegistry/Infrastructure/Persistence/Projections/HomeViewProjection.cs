using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Domain.Homes;
using CommonGround.PropertyRegistry.Domain.Homes.Events;
using JasperFx.Events;
using Marten.Events.Aggregation;

namespace CommonGround.PropertyRegistry.Infrastructure.Persistence.Projections;

/// <summary>Builds <see cref="HomeView"/> from Home events.</summary>
internal sealed class HomeViewProjection : SingleStreamProjection<HomeView, string>
{
    /// <summary>Creates the initial view from a HomeRegistered event.</summary>
    public static HomeView Create(IEvent<HomeRegistered> e) =>
        new()
        {
            Id = e.Data.AggregateId,
            LandParcelId = e.Data.LandParcelId,
            UnitType = e.Data.UnitType,
            Bedrooms = e.Data.Bedrooms,
            Bathrooms = e.Data.Bathrooms,
            IsAccessible = e.Data.IsAccessible,
            SquareFootage = e.Data.SquareFootage,
            YearBuilt = e.Data.YearBuilt,
            AssessedValue = e.Data.InitialAssessedValue,
            LastAssessedDate = e.Data.InitiallyAssessedAt,
            Status = HomeStatus.Available.DisplayName,
            RegisteredAt = e.Data.OccurredAt,
        };

    /// <summary>Applies an assessment update.</summary>
    public static void Apply(HomeView view, HomeAssessed e)
    {
        view.AssessedValue = e.AssessedValue;
        view.LastAssessedDate = e.AssessmentDate;
        view.ConditionNotes = e.ConditionNotes;
    }

    /// <summary>Applies an under-contract status change.</summary>
    public static void Apply(HomeView view, HomeUnderContract e)
    {
        view.Status = HomeStatus.UnderContract.DisplayName;
    }
}
