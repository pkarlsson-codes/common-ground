using System.Globalization;
using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.SharedKernel.Application.WorkItems;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.HomeBuyers.Application.WorkItems;

/// <summary>Derives the work items for applicants and income limits from HomeBuyers' read models.</summary>
public static class HomeBuyersWorkItems
{
    /// <summary>How far ahead a mortgage prequalification counts as expiring.</summary>
    public const int PrequalificationWindowDays = 30;

    /// <summary>A submitted application waiting for staff to approve or deny it.</summary>
    public const string ApplicationAwaitingDecision = "application-awaiting-decision";

    /// <summary>A household with an open application that hasn't finished readiness.</summary>
    public const string ReadinessStepMissing = "readiness-step-missing";

    /// <summary>A household with an open application whose prequalification expires soon.</summary>
    public const string PrequalificationExpiring = "prequalification-expiring";

    /// <summary>A home has an income limit that the limits in effect have no level for.</summary>
    public const string IncomeLimitsMissing = "income-limits-missing";

    /// <summary>The limits in effect are more than <see cref="IncomeLimitsStaleMonths"/> months old.</summary>
    public const string IncomeLimitsStale = "income-limits-stale";

    /// <summary>How old the limits in effect can get before staff are asked for new ones. HUD publishes yearly.</summary>
    public const int IncomeLimitsStaleMonths = 12;

    /// <summary>How many blocked homes the missing-limits item names before it says how many more.</summary>
    public const int BlockedHomesNamed = 3;

    /// <summary>Approve a submitted application.</summary>
    public const string ApproveApplication = "approve-application";

    /// <summary>Deny a submitted application.</summary>
    public const string DenyApplication = "deny-application";

    /// <summary>Record that the household completed housing counseling.</summary>
    public const string RecordHousingCounseling = "record-housing-counseling";

    /// <summary>Record that the household completed CLT orientation.</summary>
    public const string RecordCltOrientation = "record-clt-orientation";

    /// <summary>Record the household's mortgage prequalification.</summary>
    public const string RecordMortgagePrequalification = "record-mortgage-prequalification";

    /// <summary>Enter a new income limit schedule.</summary>
    public const string EnterIncomeLimits = "enter-income-limits";

    /// <summary>
    /// The items for an open (submitted or approved) application: a decision while it is submitted, and its
    /// household's missing readiness or expiring prequalification as of <paramref name="today"/>. Readiness of
    /// households that haven't applied isn't anyone's work yet.
    /// </summary>
    /// <param name="application">The application.</param>
    /// <param name="household">The household that applied, if its view has arrived.</param>
    /// <param name="today">The trust's today.</param>
    /// <param name="zone">The trust's time zone, for the date the application was submitted.</param>
    /// <returns>The application's work items.</returns>
    public static IEnumerable<WorkItem> ForApplication(
        HomeownerApplicationListItemView application,
        HouseholdListItemView? household,
        DateOnly today,
        IanaTimeZone zone)
    {
        ArgumentNullException.ThrowIfNull(application);
        ArgumentNullException.ThrowIfNull(zone);

        var label = household?.PrimaryMemberName ?? "Household";
        var submittedOn = zone.ToDate(application.SubmittedAt);

        if (application.Phase == ApplicationPhase.Submitted.DisplayName)
        {
            yield return new WorkItem(
                WorkArea.Applicants, ApplicationAwaitingDecision, "application", application.Id, label,
                "Submitted, waiting for a decision", null, submittedOn,
                [ApproveApplication, DenyApplication]);
        }

        if (household is null)
            yield break;

        var missing = MissingReadiness(household, today);
        if (missing.Count > 0)
        {
            yield return new WorkItem(
                WorkArea.Applicants, ReadinessStepMissing, "household", household.Id, label,
                $"Missing {string.Join(", ", missing.Select(m => m.Step))}", null, submittedOn,
                [.. missing.Select(m => m.Action)]);
        }
        else if (household.MortgagePrequalificationExpiryDate is { } expiry && expiry <= today.AddDays(PrequalificationWindowDays))
        {
            yield return new WorkItem(
                WorkArea.Applicants, PrequalificationExpiring, "household", household.Id, label,
                "Mortgage prequalification expiring", expiry, expiry.AddDays(-PrequalificationWindowDays),
                [RecordMortgagePrequalification]);
        }
    }

    /// <summary>
    /// The items for the trust's income limits on <paramref name="today"/>: missing when a home's percent has no
    /// level in the schedule in effect (or none is in effect), naming those homes; out of date when the schedule
    /// in effect is more than <see cref="IncomeLimitsStaleMonths"/> months old and no later one is entered. None
    /// when no home has an income limit.
    /// </summary>
    /// <param name="homes">The trust's homes.</param>
    /// <param name="schedules">The trust's income limit schedules, in any order.</param>
    /// <param name="today">The trust's today.</param>
    /// <returns>The income limits' work items.</returns>
    public static IEnumerable<WorkItem> ForIncomeLimits(
        IReadOnlyCollection<HomeFactsView> homes,
        IReadOnlyCollection<IncomeLimitScheduleView> schedules,
        DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(homes);
        ArgumentNullException.ThrowIfNull(schedules);

        var limited = homes.Where(h => h.IncomeLimitPercentOfAmi is not null).ToList();
        if (limited.Count == 0)
            yield break;

        var inEffect = IncomeLimitScheduleView.InEffectOn(schedules, today);
        var levels = inEffect?.Levels.Select(l => l.PercentOfAmi).ToHashSet() ?? [];
        var blocked = limited
            .Where(h => !levels.Contains(h.IncomeLimitPercentOfAmi!.Value))
            .OrderBy(h => h.Label, StringComparer.Ordinal)
            .ToList();

        if (blocked.Count > 0)
        {
            var percents = blocked.Select(h => h.IncomeLimitPercentOfAmi!.Value).Distinct().Order().Select(p => $"{p}%");
            var named = string.Join(", ", blocked.Take(BlockedHomesNamed).Select(h => h.Label));
            var more = blocked.Count > BlockedHomesNamed ? $" and {blocked.Count - BlockedHomesNamed} more" : string.Empty;
            yield return new WorkItem(
                WorkArea.Selections, IncomeLimitsMissing, "income-limits", inEffect?.Id ?? string.Empty, "Income limits",
                $"No {string.Join(", ", percents)} limits in effect; households can't be chosen for {named}{more}",
                null, today, [EnterIncomeLimits]);
        }

        if (inEffect is not null
            && inEffect.EffectiveFrom.AddMonths(IncomeLimitsStaleMonths) < today
            && !schedules.Any(s => s.EffectiveFrom > inEffect.EffectiveFrom))
        {
            var since = inEffect.EffectiveFrom.ToString("MMM d, yyyy", CultureInfo.GetCultureInfo("en-US"));
            yield return new WorkItem(
                WorkArea.Selections, IncomeLimitsStale, "income-limits", inEffect.Id, "Income limits",
                $"In effect since {since}, over {IncomeLimitsStaleMonths} months ago; HUD publishes new limits every year",
                null, inEffect.EffectiveFrom.AddMonths(IncomeLimitsStaleMonths), [EnterIncomeLimits]);
        }
    }

    private static List<(string Step, string Action)> MissingReadiness(HouseholdListItemView household, DateOnly today)
    {
        var missing = new List<(string Step, string Action)>();
        if (!household.IsHousingCounselingCompleted)
            missing.Add(("housing counseling", RecordHousingCounseling));
        if (!household.IsCltOrientationCompleted)
            missing.Add(("CLT orientation", RecordCltOrientation));
        if (!(household.MortgagePrequalificationExpiryDate >= today))
        {
            missing.Add((
                household.NeedsReverificationOn(today) ? "a current mortgage prequalification" : "mortgage prequalification",
                RecordMortgagePrequalification));
        }

        return missing;
    }
}
