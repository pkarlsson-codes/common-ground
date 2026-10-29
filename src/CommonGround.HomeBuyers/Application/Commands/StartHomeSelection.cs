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

        _ = await session
            .LoadAsync<HomeFactsView>(command.HomeId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(command.HomeId);

        var activeSelectionId = await session.Query<HomeSelectionView>()
            .Where(v => v.HomeId == command.HomeId && ActiveStatuses.Contains(v.Status))
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
            command.HomeId,
            candidates,
            activeSelectionId is null ? null : HomeSelectionId.Create(activeSelectionId),
            command.StartedBy);

        session.CreateAggregate<HomeSelection, HomeSelectionId>(selection);

        return selection.Id;
    }
}
