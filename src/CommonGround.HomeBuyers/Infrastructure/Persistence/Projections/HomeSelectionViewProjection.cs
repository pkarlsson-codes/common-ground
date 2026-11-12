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
            StartedBy = e.Data.StartedBy,
            StartedAt = e.Data.OccurredAt,
            IncomeLimitPercentOfAmi = e.Data.IncomeLimitPercentOfAmi,
            IncomeLimitsEffectiveFrom = e.Data.IncomeLimitsEffectiveFrom,
            Candidates = [.. e.Data.Candidates.Select(c => new HomeSelectionCandidateEntry
            {
                ApplicationId = c.ApplicationId,
                HouseholdId = c.HouseholdId,
                AnnualIncome = c.AnnualIncome.Amount,
                HouseholdSize = c.HouseholdSize,
                RequiresAccessibility = c.RequiresAccessibility,
                WaitingSince = c.WaitingSince,
                IncomeLimit = c.IncomeLimit?.Amount,
                IsOverIncomeLimit = c.IncomeLimit is not null && c.AnnualIncome > c.IncomeLimit,
            })],
        };

    public static void Apply(HomeSelectionView view, HouseholdSelectedByPriority e)
    {
        view.Status = HomeSelectionStatus.Completed.DisplayName;
        view.SelectedApplicationId = e.ApplicationId;
        view.SelectionMethod = "Priority";
        view.Priorities = [.. e.Priorities.Select(p => p.DisplayName)];
        view.SelectionReason = e.Reason;
        view.ClosedBy = e.SelectedBy;
        view.ClosedAt = e.OccurredAt;
    }

    public static void Apply(HomeSelectionView view, HouseholdSelectedByLottery e)
    {
        view.Status = HomeSelectionStatus.Completed.DisplayName;
        view.SelectedApplicationId = e.ApplicationId;
        view.SelectionMethod = "Lottery";
        view.LotterySeed = e.Seed;
        view.LotteryPoolApplicationIds = [.. e.PoolApplicationIds];
        view.LotteryIneligibleApplicationIds = [.. e.IneligibleApplicationIds];
        view.ClosedBy = e.DrawnBy;
        view.ClosedAt = e.OccurredAt;
    }

    public static void Apply(HomeSelectionView view, HomeSelectionPurchaseFellThrough e)
    {
        view.Status = HomeSelectionStatus.FellThrough.DisplayName;
        view.FellThroughReason = e.Reason;
        view.FellThroughDate = e.FellThroughDate;
    }

    public static void Apply(HomeSelectionView view, HomeSelectionCancelled e)
    {
        view.Status = HomeSelectionStatus.Cancelled.DisplayName;
        view.CancellationReason = e.Reason;
        view.ClosedBy = e.CancelledBy;
        view.ClosedAt = e.OccurredAt;
    }
}
