using CommonGround.HomeBuyers.Application.Commands;
using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.SharedKernel.Domain.Services;
using Marten;

namespace CommonGround.HomeBuyers.Application.Queries;

/// <summary>Returns a home's selection page: its waiting list compared against it today, and the households chosen for it.</summary>
public sealed record GetHomeSelectionOverview(string HomeId);

/// <summary>Handles <see cref="GetHomeSelectionOverview"/>.</summary>
public static class GetHomeSelectionOverviewHandler
{
    /// <summary>
    /// Compares the waiting list against the home on the trust's today, loads the home's recorded choices, and joins
    /// each household with its primary contact and prequalified amount.
    /// </summary>
    public static async Task<HomeSelectionOverview> Handle(
        GetHomeSelectionOverview request,
        IQuerySession session,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(trustClock);

        var today = await trustClock.TodayAsync(session.TenantId, cancellationToken).ConfigureAwait(false);
        var (home, comparison) = await WaitingListComparisonLoader
            .LoadAsync(session, request.HomeId, today, cancellationToken)
            .ConfigureAwait(false);

        var selections = await session.Query<HomeSelectionView>()
            .Where(v => v.HomeId == request.HomeId)
            .OrderByDescending(v => v.RecordedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var householdIds = comparison.Applications.Select(a => a.Application.HouseholdId.Value)
            .Concat(selections.SelectMany(s => s.Applications.Select(a => a.HouseholdId)))
            .Distinct()
            .ToList();
        var households = (await session
            .LoadManyAsync<HouseholdListItemView>(cancellationToken, householdIds)
            .ConfigureAwait(false))
            .ToDictionary(h => h.Id);
        var prequalifiedAmounts = (await session
            .LoadManyAsync<HouseholdView>(cancellationToken, comparison.Applications.Select(a => a.Application.HouseholdId.Value).Distinct())
            .ConfigureAwait(false))
            .Where(h => h.MortgagePrequalification is not null)
            .ToDictionary(h => h.Id, h => h.MortgagePrequalification!.ApprovedAmount);

        HouseholdContact? ContactOf(string householdId) =>
            households.GetValueOrDefault(householdId) is { } household ? HouseholdContact.From(household) : null;

        return new HomeSelectionOverview(
            home,
            home.CurrentSaleStage,
            home.IsOffered,
            today,
            comparison.IncomeLimits?.EffectiveFrom,
            comparison.HasIncomeLimitsInEffect,
            [.. comparison.Applications.Select(a => new WaitingListComparisonEntry(
                a.Application.ApplicationId.Value,
                a.Application.HouseholdId.Value,
                a.Application.AnnualIncome.Amount,
                a.Application.HouseholdSize,
                a.Application.RequiresAccessibility,
                a.Application.WaitingSince,
                a.IncomeLimit?.Amount,
                [.. a.Reasons.Select(r => r.DisplayName)],
                ContactOf(a.Application.HouseholdId.Value),
                prequalifiedAmounts.TryGetValue(a.Application.HouseholdId.Value, out var amount) ? amount : null))],
            [.. selections.Select(s => new HomeSelectionDecision(
                s,
                [.. s.Applications.Select(a => new HomeSelectionDecisionEntry(a, ContactOf(a.HouseholdId)))]))]);
    }
}
