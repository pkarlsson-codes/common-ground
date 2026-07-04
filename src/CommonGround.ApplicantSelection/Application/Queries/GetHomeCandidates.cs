using CommonGround.ApplicantSelection.Application.ReadModels;
using CommonGround.ApplicantSelection.Domain.HomeownerApplications;
using Marten;

namespace CommonGround.ApplicantSelection.Application.Queries;

/// <summary>
/// Returns homeowner applications eligible to be selected for a given home.
/// </summary>
/// <param name="HomeId">
/// The home candidates are being sourced for. Accepted for future eligibility filtering
/// (e.g. income/AMI matching against the home) — not yet used; see <see cref="GetHomeCandidatesHandler"/>.
/// </param>
/// <param name="Search">Optional free-text search across applicant fields.</param>
public sealed record GetHomeCandidates(string HomeId, string? Search = null);

/// <summary>Handles <see cref="GetHomeCandidates"/>.</summary>
public static class GetHomeCandidatesHandler
{
    /// <summary>
    /// Returns all Waitlisted-phase applications — the only phase from which an applicant
    /// can actually be selected (see <see cref="HomeownerApplication.SelectForHome"/>).
    ///
    /// Eligibility filtering against the home (income/household-size/AMI matching) is
    /// undefined today and needs a product decision; until then this returns every
    /// Waitlisted application unfiltered, ignoring <see cref="GetHomeCandidates.HomeId"/>.
    /// </summary>
    public static async Task<IReadOnlyList<HomeownerApplicationListItemView>> Handle(
        GetHomeCandidates request,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        IQueryable<HomeownerApplicationListItemView> query = session.Query<HomeownerApplicationListItemView>()
            .Where(v => v.Phase == ApplicationPhase.Waitlisted.DisplayName);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLowerInvariant();
#pragma warning disable CA1862 // ToLowerInvariant().Contains() is intentional — Marten translates this to SQL; StringComparison overload is not supported by the query provider
            query = query.Where(v =>
                v.ApplicantFullName.ToLowerInvariant().Contains(search) ||
                v.ApplicantEmail.ToLowerInvariant().Contains(search));
#pragma warning restore CA1862
        }

        return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
    }
}
