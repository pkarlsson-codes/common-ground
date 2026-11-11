using CommonGround.SharedKernel.Domain.Services;
using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using Marten;

namespace CommonGround.Homeownership.Application.Queries;

/// <summary>
/// Returns everything needed to print late notices: each lease with a notice due, or just one lease, with the trust's
/// sign-off and today's date for the letters. Not paged: a print run covers every letter.
/// </summary>
/// <param name="GroundLeaseId">Only this lease, if given; it is returned only when a notice is due.</param>
public sealed record GetLeaseFeeNoticesDue(string? GroundLeaseId = null);

/// <summary>The letters to print.</summary>
/// <param name="Today">The trust's today, the date on every letter.</param>
/// <param name="LetterSignOff">The trust's sign-off; empty for none.</param>
/// <param name="CurePeriod">The cure period a notice of default gives, for its letter.</param>
/// <param name="Notices">One lease per letter, most days overdue first.</param>
public sealed record LeaseFeeNoticesDue(
    DateOnly Today, string LetterSignOff, DefaultCurePeriod CurePeriod, IReadOnlyList<LeaseFeeBalance> Notices);

/// <summary>Handles <see cref="GetLeaseFeeNoticesDue"/>.</summary>
public static class GetLeaseFeeNoticesDueHandler
{
    /// <summary>Works out the notices due as of the trust's today.</summary>
    public static async Task<LeaseFeeNoticesDue> Handle(
        GetLeaseFeeNoticesDue request,
        IQuerySession session,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(trustClock);

        var today = await trustClock.TodayAsync(session.TenantId, cancellationToken).ConfigureAwait(false);
        var policy = await GetLeaseFeeNoticePolicyHandler.LoadAsync(session, cancellationToken).ConfigureAwait(false);
        var all = await LeaseFeeBalances.LoadAsync(session, today, policy.Schedule, cancellationToken).ConfigureAwait(false);

        return new LeaseFeeNoticesDue(
            today,
            policy.LetterSignOff,
            policy.CurePeriod,
            [.. all.Where(b => b.NoticeDue is not null
                && (request.GroundLeaseId is null || b.Lease.Id == request.GroundLeaseId))]);
    }
}
