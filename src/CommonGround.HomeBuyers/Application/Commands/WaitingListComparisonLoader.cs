using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.HomeBuyers.Domain.IncomeLimitSchedules;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Extensions;
using CommonGround.SharedKernel.Specifications;
using Marten;

namespace CommonGround.HomeBuyers.Application.Commands;

/// <summary>A home's facts and its waiting list compared against it on a date.</summary>
/// <param name="Home">The home's facts.</param>
/// <param name="Comparison">The waiting list as it is, compared against the home.</param>
public sealed record HomeComparison(HomeFactsView Home, WaitingListComparison Comparison);

/// <summary>
/// Compares the waiting list as it is against a home, from the inline read models, for both the selection page and
/// the commands that choose a household. Choosing still checks the chosen household's own aggregates.
/// </summary>
public static class WaitingListComparisonLoader
{
    /// <summary>
    /// Loads the home's facts, every application on the waiting list with its household's readiness, longest waiting
    /// first, and the income limit schedule in effect on <paramref name="asOf"/>, and compares them. An application
    /// approved after <paramref name="asOf"/> wasn't waiting yet on that date, so it is left out.
    /// </summary>
    public static async Task<HomeComparison> LoadAsync(
        IQuerySession session,
        string homeId,
        DateOnly asOf,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);

        var home = await session
            .LoadAsync<HomeFactsView>(homeId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(homeId);

        // Loaded for every home: one with no limit still records the buyer's income band from it.
        var scheduleInEffect = await LoadScheduleInEffectAsync(session, asOf, cancellationToken).ConfigureAwait(false);

        var approved = await session.Query<HomeownerApplicationView>()
            .Matching(OnWaitingList<HomeownerApplicationView>.Instance)
            .Where(v => v.ApprovedDate <= asOf)
            .OrderBy(v => v.ApprovedDate)
            .ThenBy(v => v.SubmittedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var households = (await session
            .LoadManyAsync<HouseholdListItemView>(cancellationToken, approved.Select(a => a.HouseholdId).Distinct())
            .ConfigureAwait(false))
            .ToDictionary(h => h.Id);

        var waiting = approved
            .Select(v =>
            {
                var household = households.GetValueOrDefault(v.HouseholdId);
                return new WaitingApplication(
                    HomeownerApplicationId.Create(v.Id),
                    HouseholdId.Create(v.HouseholdId),
                    Money.Create(v.AnnualIncome),
                    v.HouseholdSize,
                    v.AccessibilityRequired,
                    v.ApprovedDate ?? throw new InvalidOperationException($"Approved application {v.Id} has no approval date."),
                    HasPrimaryMember: !string.IsNullOrEmpty(household?.PrimaryMemberId),
                    IsHousingCounselingCompleted: household?.IsHousingCounselingCompleted ?? false,
                    IsCltOrientationCompleted: household?.IsCltOrientationCompleted ?? false,
                    PrequalificationExpiryDate: household?.MortgagePrequalificationExpiryDate);
            })
            .ToList();

        return new HomeComparison(
            home,
            WaitingListComparison.Create(waiting, home.IncomeLimitPercentOfAmi, scheduleInEffect?.Table, asOf));
    }

    private static async Task<IncomeLimitSchedule?> LoadScheduleInEffectAsync(
        IQuerySession session,
        DateOnly asOf,
        CancellationToken cancellationToken)
    {
        var schedules = await session.Query<IncomeLimitScheduleView>()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var inEffect = IncomeLimitScheduleView.InEffectOn(schedules, asOf);
        return inEffect is null
            ? null
            : await session
                .LoadAggregateAsync<IncomeLimitSchedule>(inEffect.Id, cancellationToken)
                .ConfigureAwait(false);
    }
}
