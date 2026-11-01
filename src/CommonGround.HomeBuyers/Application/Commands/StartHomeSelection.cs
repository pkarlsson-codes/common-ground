using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.HomeBuyers.Application.Commands;

/// <summary>Starts choosing a household for a home from the current waiting list.</summary>
public sealed record StartHomeSelection(
    string HomeId,
    UserId StartedBy);

/// <summary>Handles <see cref="StartHomeSelection"/>.</summary>
public static class StartHomeSelectionHandler
{
    private static readonly string[] ActiveStatuses =
        [HomeSelectionStatus.Open.DisplayName, HomeSelectionStatus.Completed.DisplayName];

    /// <summary>
    /// Snapshots every approved application as a candidate, waiting since its approval date,
    /// starts the selection, and returns its ID.
    /// </summary>
    public static async Task<HomeSelectionId> Handle(
        StartHomeSelection command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var selection = await StartAsync(session, command.HomeId, command.StartedBy, endedSelectionId: null, cancellationToken)
            .ConfigureAwait(false);

        return selection.Id;
    }

    /// <summary>
    /// Starts a selection for <paramref name="homeId"/> from the approved applications as of the last save.
    /// <paramref name="endedSelectionId"/> names a selection ended in this session but not yet saved, so it
    /// no longer counts as the home's active selection.
    /// </summary>
    internal static async Task<HomeSelection> StartAsync(
        IDocumentSession session,
        string homeId,
        UserId startedBy,
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
                v.AnnualIncome,
                v.HouseholdSize,
                v.AccessibilityRequired,
                v.ApprovedDate ?? throw new InvalidOperationException($"Approved application {v.Id} has no approval date.")))
            .ToList();

        var selection = HomeSelection.Start(
            homeId,
            candidates,
            EntityIds.CreateOrNull<HomeSelectionId>(activeSelectionId),
            startedBy);

        session.CreateAggregate<HomeSelection, HomeSelectionId>(selection);

        return selection;
    }
}
