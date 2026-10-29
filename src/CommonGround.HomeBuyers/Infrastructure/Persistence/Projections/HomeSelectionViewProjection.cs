using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.HomeBuyers.Domain.HomeSelections.Events;
using JasperFx.Events;
using Marten.Events.Aggregation;

namespace CommonGround.HomeBuyers.Infrastructure.Persistence.Projections;

/// <summary>Builds <see cref="HomeSelectionView"/> from HomeSelection events.</summary>
internal sealed class HomeSelectionViewProjection : SingleStreamProjection<HomeSelectionView, string>
{
    public static HomeSelectionView Create(IEvent<HomeSelectionStarted> e) =>
        new()
        {
            Id = e.Data.AggregateId,
            HomeId = e.Data.HomeId,
            Status = HomeSelectionStatus.Open.DisplayName,
        };

    public static void Apply(HomeSelectionView view, HouseholdSelectedByPriority _)
    {
        view.Status = HomeSelectionStatus.Completed.DisplayName;
    }

    public static void Apply(HomeSelectionView view, HouseholdSelectedByLottery _)
    {
        view.Status = HomeSelectionStatus.Completed.DisplayName;
    }

    public static void Apply(HomeSelectionView view, HomeSelectionCancelled _)
    {
        view.Status = HomeSelectionStatus.Cancelled.DisplayName;
    }
}
