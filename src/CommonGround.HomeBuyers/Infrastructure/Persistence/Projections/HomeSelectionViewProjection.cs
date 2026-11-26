using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.HomeBuyers.Domain.HomeSelections.Events;
using JasperFx.Events;
using Marten.Events.Aggregation;

namespace CommonGround.HomeBuyers.Infrastructure.Persistence.Projections;

/// <summary>Builds <see cref="HomeSelectionView"/> from the one event a HomeSelection stream holds.</summary>
internal sealed class HomeSelectionViewProjection : SingleStreamProjection<HomeSelectionView, string>
{
    public static HomeSelectionView Create(IEvent<HouseholdSelectedByPriority> e)
    {
        var data = e.Data;
        var view = Start(
            data.AggregateId, data.HomeId, data.ApplicationId, data.SelectedDate, data.SelectedBy, data.OccurredAt,
            data.GroundLeaseTemplateId, data.PurchasePrice, data.IncomeLimitPercentOfAmi, data.IncomeLimitsEffectiveFrom,
            data.Applications);
        view.SelectionMethod = "Priority";
        view.Priorities = [.. data.Priorities.Select(p => p.DisplayName)];
        view.SelectionReason = data.Reason;
        return view;
    }

    public static HomeSelectionView Create(IEvent<HouseholdSelectedByLottery> e)
    {
        var data = e.Data;
        var view = Start(
            data.AggregateId, data.HomeId, data.ApplicationId, data.SelectedDate, data.SelectedBy, data.OccurredAt,
            data.GroundLeaseTemplateId, data.PurchasePrice, data.IncomeLimitPercentOfAmi, data.IncomeLimitsEffectiveFrom,
            data.Applications);
        view.SelectionMethod = "Lottery";
        view.LotteryTiedApplicationIds = data.TiedApplicationIds is null ? null : [.. data.TiedApplicationIds];
        view.LotteryPoolApplicationIds = [.. data.PoolApplicationIds];
        view.LotterySeed = data.Seed;
        return view;
    }

    private static HomeSelectionView Start(
        string id,
        string homeId,
        string applicationId,
        DateOnly selectedDate,
        string selectedBy,
        DateTimeOffset recordedAt,
        string groundLeaseTemplateId,
        decimal purchasePrice,
        int? incomeLimitPercentOfAmi,
        DateOnly? incomeLimitsEffectiveFrom,
        IReadOnlyList<RecordedApplication> applications) =>
        new()
        {
            Id = id,
            HomeId = homeId,
            SelectedApplicationId = applicationId,
            SelectedHouseholdId = applications.FirstOrDefault(a => a.ApplicationId == applicationId)?.HouseholdId ?? string.Empty,
            SelectedDate = selectedDate,
            SelectedBy = selectedBy,
            RecordedAt = recordedAt,
            GroundLeaseTemplateId = groundLeaseTemplateId,
            PurchasePrice = purchasePrice,
            IncomeLimitPercentOfAmi = incomeLimitPercentOfAmi,
            IncomeLimitsEffectiveFrom = incomeLimitsEffectiveFrom,
            Applications = [.. applications.Select(a => new HomeSelectionApplicationEntry
            {
                ApplicationId = a.ApplicationId,
                HouseholdId = a.HouseholdId,
                AnnualIncome = a.AnnualIncome.Amount,
                HouseholdSize = a.HouseholdSize,
                RequiresAccessibility = a.RequiresAccessibility,
                WaitingSince = a.WaitingSince,
                IncomeLimit = a.IncomeLimit?.Amount,
                Outcome = a.Outcome.DisplayName,
                Reasons = [.. a.Reasons.Select(r => r.DisplayName)],
            })],
        };
}
