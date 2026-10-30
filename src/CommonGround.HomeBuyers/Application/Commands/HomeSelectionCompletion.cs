using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.HomeBuyers.Application.Commands;

/// <summary>A home selection candidate's current application and household.</summary>
internal sealed record CandidateApplication(HomeownerApplication Application, Household Household);

/// <summary>Shared steps for the commands that choose a household in a home selection.</summary>
internal static class HomeSelectionCompletion
{
    /// <summary>Loads the current application and household of every candidate in <paramref name="selection"/>.</summary>
    public static async Task<IReadOnlyList<CandidateApplication>> LoadCandidatesAsync(
        IDocumentSession session,
        HomeSelection selection,
        CancellationToken cancellationToken)
    {
        var candidates = new List<CandidateApplication>(selection.Candidates.Count);
        foreach (var candidate in selection.Candidates)
        {
            var application = await session
                .LoadAggregateAsync<HomeownerApplication>(candidate.ApplicationId, cancellationToken)
                .ConfigureAwait(false);
            var household = await session
                .LoadAggregateAsync<Household>(candidate.HouseholdId, cancellationToken)
                .ConfigureAwait(false);
            candidates.Add(new CandidateApplication(application, household));
        }

        return candidates;
    }

    /// <summary>
    /// Selects the chosen candidate's application for the home and passes over every other
    /// candidate who could still have been selected on <paramref name="selectedDate"/>. Candidates
    /// who withdrew or whose readiness lapsed were never really in the running, so they are not passed over.
    /// </summary>
    public static async Task CompleteAsync(
        IDocumentSession session,
        HomeSelection selection,
        IReadOnlyList<CandidateApplication> candidates,
        string groundLeaseTemplateId,
        decimal purchasePrice,
        DateOnly selectedDate,
        UserId selectedBy,
        CancellationToken cancellationToken)
    {
        var selectedApplicationId = selection.SelectedApplicationId
            ?? throw new InvalidOperationException($"Home selection {selection.Id.Value} has no chosen household.");

        var home = await session
            .LoadAsync<HomeFactsView>(selection.HomeId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(selection.HomeId);

        foreach (var (application, household) in candidates)
        {
            if (application.Id == selectedApplicationId)
            {
                application.SelectForHome(
                    household,
                    selection.HomeId,
                    home.LandParcelId,
                    groundLeaseTemplateId,
                    purchasePrice,
                    selectedDate,
                    selectedBy);
            }
            else if (application.CanBeSelected(household, selectedDate))
            {
                application.PassOver(selection.Id, selection.HomeId);
            }
            else
            {
                continue;
            }

            session.UpdateAggregate(application);
        }
    }
}
