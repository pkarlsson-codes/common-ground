using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Domain.Services;
using Marten;

namespace CommonGround.HomeBuyers.Application.Queries;

/// <summary>Returns a single home selection by ID, with its candidates' priority facts side by side.</summary>
public sealed record GetHomeSelection(HomeSelectionId SelectionId);

/// <summary>Handles <see cref="GetHomeSelection"/>.</summary>
public static class GetHomeSelectionHandler
{
    /// <summary>
    /// Loads the selection view, throwing if not found, and joins each candidate with its current
    /// application phase and household. Candidates keep their waiting-list order.
    /// </summary>
    public static async Task<HomeSelectionDetails> Handle(
        GetHomeSelection request,
        IQuerySession session,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var selection = await session.LoadAsync<HomeSelectionView>(request.SelectionId.Value, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(request.SelectionId.Value);

        var home = await session.LoadAsync<HomeFactsView>(selection.HomeId, cancellationToken).ConfigureAwait(false);

        var applications = await session
            .LoadManyAsync<HomeownerApplicationListItemView>(cancellationToken, selection.Candidates.Select(c => c.ApplicationId))
            .ConfigureAwait(false);
        var applicationsById = applications.ToDictionary(a => a.Id);

        var households = await session
            .LoadManyAsync<HouseholdListItemView>(cancellationToken, selection.Candidates.Select(c => c.HouseholdId).Distinct())
            .ConfigureAwait(false);
        var householdsById = households.ToDictionary(h => h.Id);

        var prequalifications = await session
            .LoadManyAsync<HouseholdView>(cancellationToken, selection.Candidates.Select(c => c.HouseholdId).Distinct())
            .ConfigureAwait(false);
        var prequalifiedAmountsByHousehold = prequalifications
            .Where(h => h.MortgagePrequalification is not null)
            .ToDictionary(h => h.Id, h => h.MortgagePrequalification!.ApprovedAmount);

        var today = await trustClock.TodayAsync(session.TenantId, cancellationToken).ConfigureAwait(false);

        return new HomeSelectionDetails(
            selection,
            home,
            [.. selection.Candidates.Select(c =>
            {
                var phase = applicationsById.GetValueOrDefault(c.ApplicationId)?.Phase ?? string.Empty;
                var household = householdsById.GetValueOrDefault(c.HouseholdId);
                var notSelectableReasons = NotSelectableReasons(c, phase, household, today);
                return new HomeSelectionCandidateDetails(
                    c,
                    household is null ? null : HouseholdContact.From(household),
                    phase,
                    notSelectableReasons.Count == 0,
                    prequalifiedAmountsByHousehold.TryGetValue(c.HouseholdId, out var amount) ? amount : null,
                    notSelectableReasons);
            })]);
    }

    /// <summary>Every reason the candidate can't be selected today, in words for staff. Empty when it can be selected.</summary>
    private static List<string> NotSelectableReasons(
        HomeSelectionCandidateEntry candidate,
        string phase,
        HouseholdListItemView? household,
        DateOnly today)
    {
        if (string.IsNullOrEmpty(phase))
            return ["Application not found"];
        if (household is null)
            return ["Household not found"];

        var reasons = new List<string>();
        if (candidate.IsOverIncomeLimit)
            reasons.Add("Over income limit");
        if (phase != ApplicationPhase.Approved.DisplayName)
            reasons.Add($"Application is {phase.ToLowerInvariant()}");
        if (!household.IsHousingCounselingCompleted)
            reasons.Add("Housing counseling not completed");
        if (!household.IsCltOrientationCompleted)
            reasons.Add("CLT orientation not completed");
        if (household.MortgagePrequalificationExpiryDate is null)
            reasons.Add("No mortgage prequalification");
        else if (household.MortgagePrequalificationExpiryDate < today)
            reasons.Add("Prequalification expired");

        return reasons;
    }
}
