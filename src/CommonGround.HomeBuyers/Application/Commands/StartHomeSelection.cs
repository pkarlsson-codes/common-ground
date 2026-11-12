using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.HomeBuyers.Domain.IncomeLimitSchedules;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using Marten;
using Wolverine.Marten;

namespace CommonGround.HomeBuyers.Application.Commands;

/// <summary>Starts choosing a household for a home from the current waiting list.</summary>
/// <param name="SelectionId">The new selection's ID, chosen by the caller.</param>
/// <param name="HomeId">The home to choose a household for.</param>
/// <param name="StartedBy">The staff user starting the selection.</param>
public sealed record StartHomeSelection(
    HomeSelectionId SelectionId,
    string HomeId,
    UserId StartedBy);

/// <summary>What starting a home selection decides from.</summary>
/// <param name="Candidates">Every approved application, waiting since its approval date, longest waiting first.</param>
/// <param name="ActiveSelectionId">The home's open or completed selection, if any.</param>
/// <param name="IncomeLimitPercentOfAmi">The home's income limit, or null when it has none.</param>
/// <param name="ScheduleInEffect">The income limit schedule in effect on <paramref name="Today"/>, loaded only when the home has a limit.</param>
/// <param name="Today">The trust's today.</param>
public sealed record SelectionStart(
    IReadOnlyList<SelectionCandidate> Candidates,
    HomeSelectionId? ActiveSelectionId,
    int? IncomeLimitPercentOfAmi,
    IncomeLimitSchedule? ScheduleInEffect,
    DateOnly Today)
{
    /// <summary>
    /// Whether the candidates can be judged against the home's income limit: the home has none, or the
    /// schedule in effect has its level. <see cref="HomeSelection.Start"/> refuses to start otherwise.
    /// </summary>
    public bool HasIncomeLimitsInEffect =>
        IncomeLimitPercentOfAmi is not { } percent || ScheduleInEffect?.LevelFor(percent) is not null;
}

/// <summary>Handles <see cref="StartHomeSelection"/>.</summary>
public static class StartHomeSelectionHandler
{
    private static readonly string[] ActiveStatuses =
        [HomeSelectionStatus.Open.DisplayName, HomeSelectionStatus.Completed.DisplayName];

    /// <summary>Snapshots the waiting list and finds the home's active selection.</summary>
    public static Task<SelectionStart> LoadAsync(
        StartHomeSelection command,
        IQuerySession session,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        return LoadStartAsync(session, trustClock, command.HomeId, endedSelectionId: null, cancellationToken);
    }

    /// <summary>Starts the selection with every approved application as a candidate, judged against the home's income limit.</summary>
    public static IStartStream Handle(StartHomeSelection command, SelectionStart start)
    {
        ArgumentNullException.ThrowIfNull(command);

        return Start(command.SelectionId, command.HomeId, start, command.StartedBy);
    }

    /// <summary>
    /// Snapshots the waiting list for <paramref name="homeId"/>, finds its active selection, ignoring
    /// <paramref name="endedSelectionId"/>, the selection a caller is ending in the same change, and, when
    /// the home has an income limit, the income limit schedule in effect on the trust's today.
    /// </summary>
    internal static async Task<SelectionStart> LoadStartAsync(
        IQuerySession session,
        ITrustClock trustClock,
        string homeId,
        string? endedSelectionId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(trustClock);

        var home = await session
            .LoadAsync<HomeFactsView>(homeId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(homeId);

        var today = await trustClock.TodayAsync(session.TenantId, cancellationToken).ConfigureAwait(false);
        var scheduleInEffect = home.IncomeLimitPercentOfAmi is null
            ? null
            : await LoadScheduleInEffectAsync(session, today, cancellationToken).ConfigureAwait(false);

        var activeSelectionId = await session.Query<HomeSelectionView>()
            .Where(v => v.HomeId == homeId && ActiveStatuses.Contains(v.Status) && v.Id != endedSelectionId)
            .Select(v => v.Id)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        var approved = await session.Query<HomeownerApplicationView>()
            .Where(v => v.Phase == ApplicationPhase.Approved.DisplayName)
            .OrderBy(v => v.ApprovedDate)
            .ThenBy(v => v.SubmittedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var candidates = approved
            .Select(v => SelectionCandidate.Create(
                HomeownerApplicationId.Create(v.Id),
                HouseholdId.Create(v.HouseholdId),
                Money.Create(v.AnnualIncome),
                v.HouseholdSize,
                v.AccessibilityRequired,
                v.ApprovedDate ?? throw new InvalidOperationException($"Approved application {v.Id} has no approval date."),
                incomeLimit: null))
            .ToList();

        return new SelectionStart(
            candidates,
            EntityIds.CreateOrNull<HomeSelectionId>(activeSelectionId),
            home.IncomeLimitPercentOfAmi,
            scheduleInEffect,
            today);
    }

    private static async Task<IncomeLimitSchedule?> LoadScheduleInEffectAsync(
        IQuerySession session,
        DateOnly today,
        CancellationToken cancellationToken)
    {
        var schedules = await session.Query<IncomeLimitScheduleView>()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var inEffect = IncomeLimitScheduleView.InEffectOn(schedules, today);
        return inEffect is null
            ? null
            : await session
                .LoadAggregateAsync<IncomeLimitSchedule>(inEffect.Id, cancellationToken)
                .ConfigureAwait(false);
    }

    /// <summary>Starts a selection's stream from what was loaded.</summary>
    internal static IStartStream Start(HomeSelectionId selectionId, string homeId, SelectionStart start, UserId startedBy)
    {
        ArgumentNullException.ThrowIfNull(start);

        var started = HomeSelection.Start(
            selectionId,
            homeId,
            start.Candidates,
            start.IncomeLimitPercentOfAmi,
            start.ScheduleInEffect,
            start.Today,
            start.ActiveSelectionId,
            startedBy);
        return MartenOps.StartStream<HomeSelection>(selectionId.Value, started);
    }
}
