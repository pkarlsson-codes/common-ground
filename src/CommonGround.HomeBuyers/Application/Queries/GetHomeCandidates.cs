using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using Marten;

namespace CommonGround.HomeBuyers.Application.Queries;

/// <summary>
/// Returns homeowner applications eligible to be selected for a given home.
/// </summary>
/// <param name="HomeId">
/// The home candidates are being sourced for. Accepted for future eligibility filtering
/// (e.g. income/AMI matching against the home) — not yet used; see <see cref="GetHomeCandidatesHandler"/>.
/// </param>
public sealed record GetHomeCandidates(string HomeId);

/// <summary>Handles <see cref="GetHomeCandidates"/>.</summary>
public static class GetHomeCandidatesHandler
{
    /// <summary>
    /// Returns all Approved-phase applications — the only phase from which an applicant
    /// can actually be selected (see <see cref="HomeownerApplication.SelectForHome"/>).
    ///
    /// Eligibility filtering against the home (income/household-size/AMI matching) is
    /// undefined today and needs a product decision; until then this returns every
    /// Approved application unfiltered, ignoring <see cref="GetHomeCandidates.HomeId"/>.
    /// </summary>
    public static async Task<IReadOnlyList<HomeCandidate>> Handle(
        GetHomeCandidates request,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var applications = await session.Query<HomeownerApplicationListItemView>()
            .Where(v => v.Phase == ApplicationPhase.Approved.DisplayName)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var households = await session
            .LoadManyAsync<HouseholdListItemView>(cancellationToken, applications.Select(a => a.HouseholdId).Distinct())
            .ConfigureAwait(false);
        var householdsById = households.ToDictionary(h => h.Id);

        return [.. applications.Select(a => new HomeCandidate(
            a,
            householdsById.TryGetValue(a.HouseholdId, out var household) ? HouseholdContact.From(household) : null))];
    }
}
