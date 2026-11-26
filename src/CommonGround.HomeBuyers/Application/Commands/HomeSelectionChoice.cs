using CommonGround.HomeBuyers.Application.Errors;
using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.HomeBuyers.Application.Commands;

/// <summary>What choosing a household for a home decides from.</summary>
/// <param name="Home">The home's facts, loaded for writing.</param>
/// <param name="Comparison">The waiting list as it is, compared against the home on the date of the choice.</param>
/// <param name="Today">The trust's today.</param>
public sealed record HomeChoice(HomeFactsView Home, WaitingListComparison Comparison, DateOnly Today);

/// <summary>
/// Shared steps for the commands that choose a household for a home. Choosing writes the selection, the home's facts
/// and the chosen application; the households passed over are recorded afterwards, one message each, by
/// <c>PassOverCandidatesHandler</c>, so choices for different homes that share a waiting list don't conflict.
/// </summary>
public static class HomeSelectionChoice
{
    /// <summary>Loads the home's facts, for writing, and the waiting list compared against it on <paramref name="selectedDate"/>.</summary>
    public static async Task<HomeChoice> LoadAsync(
        IDocumentSession session,
        ITrustClock trustClock,
        string homeId,
        DateOnly selectedDate,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(trustClock);

        var today = await trustClock.TodayAsync(session.TenantId, cancellationToken).ConfigureAwait(false);
        var loaded = await WaitingListComparisonLoader
            .LoadAsync(session, homeId, selectedDate, cancellationToken)
            .ConfigureAwait(false);

        return new HomeChoice(loaded.Home, loaded.Comparison, today);
    }

    /// <summary>
    /// Refuses a choice for a home that isn't offered, or dated before it was. A home a household was chosen for counts
    /// as chosen until PropertyRegistry hears of it, so a second choice is refused even before then.
    /// </summary>
    public static void EnsureOffered(HomeFactsView home, DateOnly selectedDate)
    {
        ArgumentNullException.ThrowIfNull(home);

        if (!home.IsOffered)
            throw BusinessRuleViolation.HomeSelection.Choose.HomeNotAvailable(home.Id, home.CurrentSaleStage);

        if (home.SaleStageSince is { } availableSince && selectedDate < availableSince)
            throw BusinessRuleViolation.HomeSelection.Choose.SelectedDateBeforeAvailable(selectedDate, availableSince);
    }

    /// <summary>
    /// Starts the selection's stream with <paramref name="chosen"/>, marks the home as chosen, and selects the chosen
    /// application for the home. The home's facts are stored with optimistic concurrency, so of two choices racing for
    /// the same home only one commits.
    /// </summary>
    public static async Task RecordAsync(
        IDocumentSession session,
        HomeChoice choice,
        HomeSelectionId selectionId,
        DomainEvent chosen,
        HomeownerApplicationId applicationId,
        SelectionTerms terms,
        UserId selectedBy,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(choice);
        ArgumentNullException.ThrowIfNull(chosen);
        ArgumentNullException.ThrowIfNull(terms);

        session.Events.StartStream<HomeSelection>(selectionId.Value, chosen);

        choice.Home.LatestSelectionId = selectionId.Value;
        session.Update(choice.Home);

        var application = await session.Events
            .FetchForWriting<HomeownerApplication>(applicationId.Value, cancellationToken)
            .ConfigureAwait(false);
        var household = await session
            .LoadAggregateAsync<Household>(application.Aggregate!.HouseholdId, cancellationToken)
            .ConfigureAwait(false);

        application.Decide(a => a.SelectForHome(
            household,
            selectionId,
            choice.Home.Id,
            choice.Home.LandParcelId,
            terms.GroundLeaseTemplateId,
            terms.PurchasePrice,
            terms.SelectedDate,
            selectedBy,
            choice.Comparison.IncomeLimitPercentOfAmi,
            choice.Comparison.IncomeLimits));
    }
}
