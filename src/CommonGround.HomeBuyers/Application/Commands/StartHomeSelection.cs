using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
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
public sealed record SelectionStart(IReadOnlyList<SelectionCandidate> Candidates, HomeSelectionId? ActiveSelectionId);

/// <summary>Handles <see cref="StartHomeSelection"/>.</summary>
public static class StartHomeSelectionHandler
{
    private static readonly string[] ActiveStatuses =
        [HomeSelectionStatus.Open.DisplayName, HomeSelectionStatus.Completed.DisplayName];

    /// <summary>Snapshots the waiting list and finds the home's active selection.</summary>
    public static Task<SelectionStart> LoadAsync(
        StartHomeSelection command,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        return LoadStartAsync(session, command.HomeId, endedSelectionId: null, cancellationToken);
    }

    /// <summary>Starts the selection with every approved application as a candidate.</summary>
    public static IStartStream Handle(StartHomeSelection command, SelectionStart start)
    {
        ArgumentNullException.ThrowIfNull(command);

        return Start(command.SelectionId, command.HomeId, start, command.StartedBy);
    }

    /// <summary>
    /// Snapshots the waiting list for <paramref name="homeId"/> and finds its active selection, ignoring
    /// <paramref name="endedSelectionId"/>, the selection a caller is ending in the same change.
    /// </summary>
    internal static async Task<SelectionStart> LoadStartAsync(
        IQuerySession session,
        string homeId,
        string? endedSelectionId,
        CancellationToken cancellationToken)
    {
        _ = await session
            .LoadAsync<HomeFactsView>(homeId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(homeId);

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
                v.ApprovedDate ?? throw new InvalidOperationException($"Approved application {v.Id} has no approval date.")))
            .ToList();

        return new SelectionStart(candidates, EntityIds.CreateOrNull<HomeSelectionId>(activeSelectionId));
    }

    /// <summary>Starts a selection's stream from what was loaded.</summary>
    internal static IStartStream Start(HomeSelectionId selectionId, string homeId, SelectionStart start, UserId startedBy)
    {
        ArgumentNullException.ThrowIfNull(start);

        var started = HomeSelection.Start(selectionId, homeId, start.Candidates, start.ActiveSelectionId, startedBy);
        return MartenOps.StartStream<HomeSelection>(selectionId.Value, started);
    }
}
