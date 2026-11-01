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

/// <summary>A home selection candidate's application, fetched for writing, and their household.</summary>
/// <param name="Application">The candidate's application stream.</param>
/// <param name="Household">The candidate's household, read-only.</param>
public sealed record CandidateApplication(IEventStream<HomeownerApplication> Application, Household Household);

/// <summary>What choosing a household in a home selection reads and writes.</summary>
/// <param name="Selection">The selection's stream.</param>
/// <param name="Candidates">Every candidate's application stream and household.</param>
/// <param name="Home">The home being selected for.</param>
public sealed record SelectionCompletion(
    IEventStream<HomeSelection> Selection,
    IReadOnlyList<CandidateApplication> Candidates,
    HomeFactsView Home)
{
    /// <summary>Gets the selection as it was loaded.</summary>
    public HomeSelection CurrentSelection =>
        Selection.Aggregate ?? throw new AggregateNotFoundException(typeof(HomeSelection), Selection.Key!);
}

/// <summary>Shared steps for the commands that choose a household in a home selection.</summary>
public static class HomeSelectionCompletion
{
    /// <summary>Fetches the selection and every candidate's application for writing, with their households and the home.</summary>
    public static async Task<SelectionCompletion> LoadAsync(
        IDocumentSession session,
        HomeSelectionId selectionId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);

        var selection = await session.Events
            .FetchForWriting<HomeSelection>(selectionId.Value, cancellationToken)
            .ConfigureAwait(false);
        var current = selection.Aggregate ?? throw new AggregateNotFoundException(typeof(HomeSelection), selectionId.Value);

        var candidates = new List<CandidateApplication>(current.Candidates.Count);
        foreach (var candidate in current.Candidates)
        {
            var application = await session.Events
                .FetchForWriting<HomeownerApplication>(candidate.ApplicationId.Value, cancellationToken)
                .ConfigureAwait(false);
            var household = await session
                .LoadAggregateAsync<Household>(candidate.HouseholdId, cancellationToken)
                .ConfigureAwait(false);
            candidates.Add(new CandidateApplication(application, household));
        }

        var home = await session
            .LoadAsync<HomeFactsView>(current.HomeId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(current.HomeId);

        return new SelectionCompletion(selection, candidates, home);
    }

    /// <summary>
    /// Selects the chosen candidate's application for the home and passes over every other
    /// candidate who could still have been selected on <paramref name="selectedDate"/>. Candidates
    /// who withdrew or whose readiness lapsed were never really in the running, so they are not passed over.
    /// </summary>
    public static void Complete(
        SelectionCompletion completion,
        HomeownerApplicationId selectedApplicationId,
        string groundLeaseTemplateId,
        decimal purchasePrice,
        DateOnly selectedDate,
        UserId selectedBy)
    {
        ArgumentNullException.ThrowIfNull(completion);

        var selection = completion.CurrentSelection;
        foreach (var (application, household) in completion.Candidates)
        {
            application.Decide(candidate =>
            {
                if (candidate.Id == selectedApplicationId)
                {
                    return candidate.SelectForHome(
                        household,
                        selection.HomeId,
                        completion.Home.LandParcelId,
                        groundLeaseTemplateId,
                        purchasePrice,
                        selectedDate,
                        selectedBy);
                }

                return candidate.CanBeSelected(household, selectedDate)
                    ? candidate.PassOver(selection.Id, selection.HomeId)
                    : null;
            });
        }
    }
}
