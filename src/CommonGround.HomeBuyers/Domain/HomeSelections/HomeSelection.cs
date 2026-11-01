using CommonGround.HomeBuyers.Application.Errors;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeSelections.Events;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.HomeSelections;

/// <summary>
/// Staff choosing which household on the waiting list gets a home, either by citing
/// <see cref="SelectionPriority"/> values or by lottery. Staff can cancel it without choosing
/// anyone, after which a new selection can start for the same home. A new selection can also start once
/// the purchase a completed selection led to falls through.
/// </summary>
public sealed class HomeSelection : AggregateRoot<HomeSelectionId>
{
#pragma warning disable CS8618 // State is set by Create from the stream's first event.
    private HomeSelection() { }
#pragma warning restore CS8618

    public string HomeId { get; private set; }
    public HomeSelectionStatus Status { get; private set; }
    public UserId StartedBy { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public IReadOnlyList<SelectionCandidate> Candidates { get; private set; }
    public HomeownerApplicationId? SelectedApplicationId { get; private set; }
    public string? CancellationReason { get; private set; }

    /// <summary>
    /// Starts choosing a household for a home from a snapshot of the waiting list. Rejected when
    /// <paramref name="activeSelectionId"/> (the home's open or completed selection, if any) is set,
    /// or when nobody is waiting.
    /// </summary>
    public static HomeSelectionStarted Start(
        HomeSelectionId id,
        string homeId,
        IReadOnlyList<SelectionCandidate> candidates,
        HomeSelectionId? activeSelectionId,
        UserId startedBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);
        ArgumentNullException.ThrowIfNull(candidates);

        if (activeSelectionId is not null)
            throw BusinessRuleViolation.HomeSelection.Start.HomeHasActiveSelection(homeId, activeSelectionId.Value);

        if (candidates.Count == 0)
            throw BusinessRuleViolation.HomeSelection.Start.WaitingListEmpty(homeId);

        return new HomeSelectionStarted(
            id.Value,
            homeId,
            [.. candidates.Select(c => new SnapshotCandidate(
                c.ApplicationId.Value,
                c.HouseholdId.Value,
                c.AnnualIncome,
                c.HouseholdSize,
                c.RequiresAccessibility,
                c.WaitingSince))],
            startedBy.Value);
    }

    /// <summary>
    /// Chooses <paramref name="applicationId"/>, citing the <paramref name="priorities"/> that put
    /// it ahead of the other candidates and explaining the choice in <paramref name="reason"/>.
    /// </summary>
    public HouseholdSelectedByPriority SelectByPriority(
        HomeownerApplicationId applicationId,
        IReadOnlyCollection<SelectionPriority> priorities,
        string reason,
        UserId selectedBy)
    {
        ArgumentNullException.ThrowIfNull(priorities);
        if (priorities.Count == 0)
            throw BusinessRuleViolation.HomeSelection.SelectByPriority.PrioritiesRequired();
        if (string.IsNullOrWhiteSpace(reason))
            throw BusinessRuleViolation.HomeSelection.SelectByPriority.ReasonRequired();

        EnsureOpen();
        EnsureCandidate(applicationId);

        return new HouseholdSelectedByPriority(
            Id.Value,
            applicationId.Value,
            [.. priorities.Distinct().Select(p => p.DisplayName)],
            reason,
            selectedBy.Value);
    }

    /// <summary>
    /// Chooses a candidate by lottery. The draw is over <paramref name="tiedApplicationIds"/> when
    /// staff found some candidates tied, otherwise every candidate, leaving out anyone not in
    /// <paramref name="eligibleApplicationIds"/> so the winner can always be selected.
    /// <paramref name="seed"/> is recorded so the draw can be re-run and checked.
    /// </summary>
    public HouseholdSelectedByLottery DrawLottery(
        IReadOnlyCollection<HomeownerApplicationId>? tiedApplicationIds,
        IReadOnlySet<HomeownerApplicationId> eligibleApplicationIds,
        int seed,
        UserId drawnBy)
    {
        ArgumentNullException.ThrowIfNull(eligibleApplicationIds);

        EnsureOpen();

        IReadOnlyCollection<HomeownerApplicationId> requested;
        if (tiedApplicationIds is null)
        {
            requested = [.. Candidates.Select(c => c.ApplicationId)];
        }
        else
        {
            requested = [.. tiedApplicationIds.Distinct()];
            if (requested.Count < 2)
                throw BusinessRuleViolation.HomeSelection.DrawLottery.TiedPoolTooSmall(requested.Count);
            foreach (var applicationId in requested)
                EnsureCandidate(applicationId);
        }

        var pool = requested.Where(eligibleApplicationIds.Contains).ToList();
        var ineligible = requested.Where(id => !eligibleApplicationIds.Contains(id)).ToList();
        if (pool.Count == 0)
            throw BusinessRuleViolation.HomeSelection.DrawLottery.NoEligibleCandidates(requested.Count);

        var winner = SelectionLottery.Draw(pool, seed);

        return new HouseholdSelectedByLottery(
            Id.Value,
            winner.Value,
            [.. pool.Select(id => id.Value).Order(StringComparer.Ordinal)],
            [.. ineligible.Select(id => id.Value).Order(StringComparer.Ordinal)],
            seed,
            drawnBy.Value);
    }

    /// <summary>Cancels the selection without choosing anyone, freeing the home for a new selection.</summary>
    public HomeSelectionCancelled Cancel(string reason, UserId cancelledBy)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw BusinessRuleViolation.HomeSelection.Cancel.ReasonRequired();

        EnsureOpen();

        return new HomeSelectionCancelled(Id.Value, reason, cancelledBy.Value);
    }

    /// <summary>Records that the purchase the chosen household started fell through, freeing the home for a new selection.</summary>
    public HomeSelectionPurchaseFellThrough RecordPurchaseFellThrough(string reason, DateOnly fellThroughDate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        if (Status != HomeSelectionStatus.Completed)
            throw BusinessRuleViolation.HomeSelection.RecordPurchaseFellThrough.NotCompleted(Status);

        return new HomeSelectionPurchaseFellThrough(Id.Value, reason, fellThroughDate);
    }

    private void EnsureOpen()
    {
        if (Status != HomeSelectionStatus.Open)
            throw BusinessRuleViolation.HomeSelection.Modify.NotOpen(Status);
    }

    private void EnsureCandidate(HomeownerApplicationId applicationId)
    {
        if (!Candidates.Any(c => c.ApplicationId == applicationId))
            throw BusinessRuleViolation.HomeSelection.Modify.NotACandidate(applicationId);
    }

    /// <summary>Starts a home selection from its first event.</summary>
    public static HomeSelection Create(HomeSelectionStarted e)
    {
        ArgumentNullException.ThrowIfNull(e);

        var homeSelection = new HomeSelection();
        homeSelection.Apply(e);
        return homeSelection;
    }

    private void Apply(HomeSelectionStarted e)
    {
        Id = HomeSelectionId.Create(e.AggregateId);
        HomeId = e.HomeId;
        Status = HomeSelectionStatus.Open;
        StartedBy = UserId.Create(e.StartedBy);
        StartedAt = e.OccurredAt;
        Candidates = e.Candidates
            .Select(c => SelectionCandidate.Create(
                HomeownerApplicationId.Create(c.ApplicationId),
                HouseholdId.Create(c.HouseholdId),
                c.AnnualIncome,
                c.HouseholdSize,
                c.RequiresAccessibility,
                c.WaitingSince))
            .ToList();
    }

    /// <summary>Applies <see cref="HouseholdSelectedByPriority"/>.</summary>
    public void Apply(HouseholdSelectedByPriority e)
    {
        SelectedApplicationId = HomeownerApplicationId.Create(e.ApplicationId);
        Status = HomeSelectionStatus.Completed;
    }

    /// <summary>Applies <see cref="HouseholdSelectedByLottery"/>.</summary>
    public void Apply(HouseholdSelectedByLottery e)
    {
        SelectedApplicationId = HomeownerApplicationId.Create(e.ApplicationId);
        Status = HomeSelectionStatus.Completed;
    }

    /// <summary>Applies <see cref="HomeSelectionPurchaseFellThrough"/>.</summary>
    public void Apply(HomeSelectionPurchaseFellThrough _)
    {
        Status = HomeSelectionStatus.FellThrough;
    }

    /// <summary>Applies <see cref="HomeSelectionCancelled"/>.</summary>
    public void Apply(HomeSelectionCancelled e)
    {
        CancellationReason = e.Reason;
        Status = HomeSelectionStatus.Cancelled;
    }
}
