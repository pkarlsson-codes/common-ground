using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.SharedKernel.Domain.Services;
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
    /// Returns Approved-phase applications whose household has completed readiness and holds a
    /// prequalification valid today — the same conditions <see cref="HomeownerApplication.SelectForHome"/>
    /// enforces.
    ///
    /// Eligibility filtering against the home (income/household-size/AMI matching) is
    /// undefined today and needs a product decision; until then <see cref="GetHomeCandidates.HomeId"/>
    /// is ignored.
    /// </summary>
    public static async Task<IReadOnlyList<HomeCandidate>> Handle(
        GetHomeCandidates request,
        IQuerySession session,
        ITrustClock trustClock,
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
        var today = await trustClock.TodayAsync(session.TenantId, cancellationToken).ConfigureAwait(false);
        var readyHouseholds = households
            .Where(h => h.IsReadyForSelectionOn(today))
            .ToDictionary(h => h.Id);

        return [.. applications
            .Where(a => readyHouseholds.ContainsKey(a.HouseholdId))
            .Select(a => new HomeCandidate(a, HouseholdContact.From(readyHouseholds[a.HouseholdId])))];
    }
}
