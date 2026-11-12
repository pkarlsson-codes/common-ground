using CommonGround.HomeBuyers.Application.Errors;
using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Extensions;
using JasperFx.Events;
using Marten;

namespace CommonGround.HomeBuyers.Application.Commands;

/// <summary>The chosen candidate's application, fetched for writing, with their household and the home.</summary>
/// <param name="Application">The chosen candidate's application stream.</param>
/// <param name="Household">The chosen candidate's household, read-only.</param>
/// <param name="Home">The home being selected for.</param>
/// <param name="Candidate">The chosen candidate in the selection.</param>
public sealed record ChosenApplication(
    IEventStream<HomeownerApplication> Application,
    Household Household,
    HomeFactsView Home,
    SelectionCandidate Candidate);

/// <summary>
/// Shared steps for the commands that choose a household in a home selection. Completing a
/// selection writes only the selection and the chosen application; the other candidates are
/// passed over afterwards, one message each, by <c>PassOverCandidatesHandler</c>. Selections
/// that share candidates can then complete at the same time without conflicting.
/// </summary>
public static class HomeSelectionCompletion
{
    /// <summary>Fetches the selection for writing.</summary>
    public static async Task<IEventStream<HomeSelection>> FetchSelectionAsync(
        IDocumentSession session,
        HomeSelectionId selectionId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);

        var selection = await session.Events
            .FetchForWriting<HomeSelection>(selectionId.Value, cancellationToken)
            .ConfigureAwait(false);
        _ = selection.Aggregate ?? throw new AggregateNotFoundException(typeof(HomeSelection), selectionId.Value);
        return selection;
    }

    /// <summary>Fetches the chosen candidate's application for writing, with their household and the home.</summary>
    public static async Task<ChosenApplication> FetchChosenAsync(
        IDocumentSession session,
        HomeSelection selection,
        HomeownerApplicationId applicationId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(selection);

        var candidate = selection.Candidates.FirstOrDefault(c => c.ApplicationId == applicationId)
            ?? throw BusinessRuleViolation.HomeSelection.Modify.NotACandidate(applicationId);

        var application = await session.Events
            .FetchForWriting<HomeownerApplication>(applicationId.Value, cancellationToken)
            .ConfigureAwait(false);
        var household = await session
            .LoadAggregateAsync<Household>(candidate.HouseholdId, cancellationToken)
            .ConfigureAwait(false);
        var home = await session
            .LoadAsync<HomeFactsView>(selection.HomeId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(selection.HomeId);

        return new ChosenApplication(application, household, home, candidate);
    }

    /// <summary>Selects the chosen candidate's application for the home.</summary>
    public static void Complete(
        HomeSelection selection,
        ChosenApplication chosen,
        string groundLeaseTemplateId,
        decimal purchasePrice,
        DateOnly selectedDate,
        UserId selectedBy)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(chosen);

        chosen.Application.Decide(application => application.SelectForHome(
            chosen.Household,
            selection.Id,
            selection.HomeId,
            chosen.Home.LandParcelId,
            groundLeaseTemplateId,
            purchasePrice,
            selectedDate,
            selectedBy,
            chosen.Candidate,
            selection.IncomeLimitLevel,
            selection.IncomeLimitsEffectiveFrom));
    }
}
