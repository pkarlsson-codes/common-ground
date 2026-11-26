using CommonGround.HomeBuyers.Application.Errors;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeSelections.Events;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.HomeBuyers.Domain.HomeSelections;

/// <summary>
/// Staff choosing which household on the waiting list gets a home, either by citing <see cref="SelectionPriority"/>
/// values or by lottery. A selection is one decision, recorded once with the whole waiting list as it was compared
/// against the home, and never changes afterwards. Whether the home can be chosen for at all is the home's sale
/// stage, not anything a selection records; whatever happens to the purchase later is the purchase's.
/// </summary>
public sealed class HomeSelection : AggregateRoot<HomeSelectionId>
{
#pragma warning disable CS8618 // State is set by Create from the stream's first event.
    private HomeSelection() { }
#pragma warning restore CS8618

    public string HomeId { get; private set; }
    public HomeownerApplicationId SelectedApplicationId { get; private set; }
    public DateOnly SelectedDate { get; private set; }

    /// <summary>
    /// Chooses <paramref name="applicationId"/> from <paramref name="comparison"/>, citing the <paramref name="priorities"/>
    /// that put it ahead of the others and explaining the choice in <paramref name="reason"/>. Rejected when the
    /// household can't be chosen on the date.
    /// </summary>
    /// <param name="id">The new selection's ID.</param>
    /// <param name="homeId">The home.</param>
    /// <param name="comparison">The waiting list compared against the home on <see cref="SelectionTerms.SelectedDate"/>.</param>
    /// <param name="applicationId">The chosen application.</param>
    /// <param name="priorities">The priorities that put the household first; at least one.</param>
    /// <param name="reason">Staff's explanation of the choice.</param>
    /// <param name="terms">The purchase's terms and the date of the choice.</param>
    /// <param name="today">The trust's today; the date of the choice can't be after it.</param>
    /// <param name="selectedBy">The staff user choosing.</param>
    /// <returns>The choice.</returns>
    public static HouseholdSelectedByPriority SelectByPriority(
        HomeSelectionId id,
        string homeId,
        WaitingListComparison comparison,
        HomeownerApplicationId applicationId,
        IReadOnlyCollection<SelectionPriority> priorities,
        string reason,
        SelectionTerms terms,
        DateOnly today,
        UserId selectedBy)
    {
        ArgumentNullException.ThrowIfNull(priorities);
        if (priorities.Count == 0)
            throw BusinessRuleViolation.HomeSelection.SelectByPriority.PrioritiesRequired();
        if (string.IsNullOrWhiteSpace(reason))
            throw BusinessRuleViolation.HomeSelection.SelectByPriority.ReasonRequired();
        EnsureCanChoose(homeId, comparison, terms, today);

        var chosen = comparison.Find(applicationId)
            ?? throw BusinessRuleViolation.HomeSelection.Choose.NotOnWaitingList(applicationId);
        if (!chosen.CanBeChosen)
            throw BusinessRuleViolation.HomeSelection.Choose.CannotBeChosen(applicationId, chosen.Reasons);

        return new HouseholdSelectedByPriority(
            id.Value,
            homeId,
            applicationId.Value,
            terms.SelectedDate,
            selectedBy.Value,
            terms.GroundLeaseTemplateId,
            terms.PurchasePrice,
            comparison.IncomeLimitPercentOfAmi,
            comparison.IncomeLimits?.EffectiveFrom,
            comparison.IncomeLimits?.ToEntries(),
            Record(comparison, applicationId),
            [.. priorities.Distinct()],
            reason.Trim());
    }

    /// <summary>
    /// Chooses a household from <paramref name="comparison"/> by lottery. The draw is over
    /// <paramref name="tiedApplicationIds"/> when staff found some households tied, otherwise the whole waiting list,
    /// leaving out anyone who can't be chosen on the date so the winner can always be chosen.
    /// <paramref name="seed"/> is recorded so the draw can be re-run and checked.
    /// </summary>
    /// <param name="id">The new selection's ID.</param>
    /// <param name="homeId">The home.</param>
    /// <param name="comparison">The waiting list compared against the home on <see cref="SelectionTerms.SelectedDate"/>.</param>
    /// <param name="tiedApplicationIds">The applications staff found tied, at least two; <see langword="null"/> to draw from the whole waiting list.</param>
    /// <param name="seed">The draw's seed.</param>
    /// <param name="terms">The purchase's terms and the date of the choice.</param>
    /// <param name="today">The trust's today; the date of the choice can't be after it.</param>
    /// <param name="drawnBy">The staff user drawing.</param>
    /// <returns>The choice.</returns>
    public static HouseholdSelectedByLottery DrawLottery(
        HomeSelectionId id,
        string homeId,
        WaitingListComparison comparison,
        IReadOnlyCollection<HomeownerApplicationId>? tiedApplicationIds,
        int seed,
        SelectionTerms terms,
        DateOnly today,
        UserId drawnBy)
    {
        EnsureCanChoose(homeId, comparison, terms, today);

        IReadOnlyList<ComparedApplication> requested;
        if (tiedApplicationIds is null)
        {
            requested = comparison.Applications;
        }
        else
        {
            var tied = tiedApplicationIds.Distinct().ToList();
            if (tied.Count < 2)
                throw BusinessRuleViolation.HomeSelection.DrawLottery.TiedPoolTooSmall(tied.Count);
            requested = [.. tied.Select(applicationId => comparison.Find(applicationId)
                ?? throw BusinessRuleViolation.HomeSelection.Choose.NotOnWaitingList(applicationId))];
        }

        var pool = requested.Where(a => a.CanBeChosen).Select(a => a.Application.ApplicationId).ToList();
        if (pool.Count == 0)
            throw BusinessRuleViolation.HomeSelection.DrawLottery.NoEligibleCandidates(requested.Count);

        var winner = SelectionLottery.Draw(pool, seed);

        return new HouseholdSelectedByLottery(
            id.Value,
            homeId,
            winner.Value,
            terms.SelectedDate,
            drawnBy.Value,
            terms.GroundLeaseTemplateId,
            terms.PurchasePrice,
            comparison.IncomeLimitPercentOfAmi,
            comparison.IncomeLimits?.EffectiveFrom,
            comparison.IncomeLimits?.ToEntries(),
            Record(comparison, winner),
            tiedApplicationIds is null
                ? null
                : [.. requested.Select(a => a.Application.ApplicationId.Value).Order(StringComparer.Ordinal)],
            [.. pool.Select(applicationId => applicationId.Value).Order(StringComparer.Ordinal)],
            seed);
    }

    private static void EnsureCanChoose(string homeId, WaitingListComparison comparison, SelectionTerms terms, DateOnly today)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);
        ArgumentNullException.ThrowIfNull(comparison);
        ArgumentNullException.ThrowIfNull(terms);
        if (comparison.AsOf != terms.SelectedDate)
            throw new ArgumentException("The waiting list must be compared on the date of the choice.", nameof(comparison));

        if (terms.SelectedDate > today)
            throw BusinessRuleViolation.HomeSelection.Choose.SelectedDateInFuture(terms.SelectedDate, today);

        if (comparison.Applications.Count == 0)
            throw BusinessRuleViolation.HomeSelection.Choose.WaitingListEmpty(homeId);

        if (!comparison.HasIncomeLimitsInEffect)
            throw BusinessRuleViolation.HomeSelection.Choose.NoIncomeLimitsInEffect(
                homeId, comparison.IncomeLimitPercentOfAmi!.Value, terms.SelectedDate);
    }

    private static List<RecordedApplication> Record(WaitingListComparison comparison, HomeownerApplicationId chosen) =>
        [.. comparison.Applications.Select(a => new RecordedApplication(
            a.Application.ApplicationId.Value,
            a.Application.HouseholdId.Value,
            a.Application.AnnualIncome,
            a.Application.HouseholdSize,
            a.Application.RequiresAccessibility,
            a.Application.WaitingSince,
            a.IncomeLimit,
            a.Application.ApplicationId == chosen ? SelectionOutcome.Chosen
                : a.CanBeChosen ? SelectionOutcome.PassedOver
                : SelectionOutcome.CouldNotBeChosen,
            a.Reasons))];

    /// <summary>Starts a home selection from its only event.</summary>
    public static HomeSelection Create(HouseholdSelectedByPriority e)
    {
        ArgumentNullException.ThrowIfNull(e);

        return new HomeSelection
        {
            Id = HomeSelectionId.Create(e.AggregateId),
            HomeId = e.HomeId,
            SelectedApplicationId = HomeownerApplicationId.Create(e.ApplicationId),
            SelectedDate = e.SelectedDate,
        };
    }

    /// <summary>Starts a home selection from its only event.</summary>
    public static HomeSelection Create(HouseholdSelectedByLottery e)
    {
        ArgumentNullException.ThrowIfNull(e);

        return new HomeSelection
        {
            Id = HomeSelectionId.Create(e.AggregateId),
            HomeId = e.HomeId,
            SelectedApplicationId = HomeownerApplicationId.Create(e.ApplicationId),
            SelectedDate = e.SelectedDate,
        };
    }
}
