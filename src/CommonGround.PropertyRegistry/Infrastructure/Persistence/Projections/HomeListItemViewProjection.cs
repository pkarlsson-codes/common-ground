using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Domain.Homes.Events;
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
            UnitDesignation = e.Data.UnitDesignation,
            UnitType = e.Data.UnitType.DisplayName,
            Bedrooms = e.Data.Bedrooms,
            Bathrooms = e.Data.Bathrooms,
            SquareFootage = e.Data.SquareFootage,
            AssessedValue = e.Data.InitialAssessedValue,
        };

    /// <summary>Applies an assessment update to the assessed value.</summary>
    public static void Apply(HomeListItemView view, HomeAssessed e)
    {
        view.AssessedValue = e.AssessedValue;
    }
}
