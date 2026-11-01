using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Domain.Homes;
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
            UnitType = e.Data.UnitType,
            Bedrooms = e.Data.Bedrooms,
            Bathrooms = e.Data.Bathrooms,
            SquareFootage = e.Data.SquareFootage,
            AssessedValue = e.Data.InitialAssessedValue,
            Status = HomeStatus.Available.DisplayName,
        };

    /// <summary>Applies an assessment update to the assessed value.</summary>
    public static void Apply(HomeListItemView view, HomeAssessed e)
    {
        view.AssessedValue = e.AssessedValue;
    }

    public static void Apply(HomeListItemView view, HomeSelectionOpened e) =>
        view.Status = HomeStatus.InSelection.DisplayName;

    public static void Apply(HomeListItemView view, HomeSelectionClosedWithoutSale e) =>
        view.Status = HomeStatus.Available.DisplayName;

    public static void Apply(HomeListItemView view, HomeUnderContract e) =>
        view.Status = HomeStatus.UnderContract.DisplayName;

    public static void Apply(HomeListItemView view, HomeContractReleased e) =>
        view.Status = HomeStatus.Available.DisplayName;

    public static void Apply(HomeListItemView view, HomeLeased e) =>
        view.Status = HomeStatus.Leased.DisplayName;
}
