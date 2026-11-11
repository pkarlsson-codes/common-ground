using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.Homeownership.Domain.LeaseFeeNoticePolicies;
using Marten;

namespace CommonGround.Homeownership.Application.Queries;

/// <summary>Gets the trust's lease fee notice policy, or the defaults when it never set one.</summary>
public sealed record GetLeaseFeeNoticePolicy;

/// <summary>A trust's lease fee notice policy as staff see it.</summary>
/// <param name="Schedule">When each notice is due.</param>
/// <param name="CurePeriod">The cure period a notice of default gives.</param>
/// <param name="LetterSignOff">The lines printed at the end of every letter; empty for none.</param>
/// <param name="IsDefault">Whether the trust never set a policy, so these are the defaults.</param>
public sealed record LeaseFeeNoticePolicyDetails(
    LeaseFeeNoticeSchedule Schedule, DefaultCurePeriod CurePeriod, string LetterSignOff, bool IsDefault)
{
    /// <summary>Gets the policy a trust has before it sets one.</summary>
    public static LeaseFeeNoticePolicyDetails Default { get; } = new(LeaseFeeNoticeSchedule.Default, DefaultCurePeriod.Default, string.Empty, true);
}

/// <summary>Handles <see cref="GetLeaseFeeNoticePolicy"/>.</summary>
public static class GetLeaseFeeNoticePolicyHandler
{
    /// <summary>Rebuilds the trust's policy from its stream: a handful of events, so no view is kept.</summary>
    public static Task<LeaseFeeNoticePolicyDetails> Handle(
        GetLeaseFeeNoticePolicy query,
        IQuerySession session,
        CancellationToken cancellationToken) =>
        LoadAsync(session, cancellationToken);

    /// <summary>Loads the policy of the session's trust.</summary>
    public static async Task<LeaseFeeNoticePolicyDetails> LoadAsync(IQuerySession session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);

        var policy = await session.Events
            .AggregateStreamAsync<LeaseFeeNoticePolicy>(LeaseFeeNoticePolicyId.ForTrust(session.TenantId).Value, token: cancellationToken)
            .ConfigureAwait(false);

        return policy is null
            ? LeaseFeeNoticePolicyDetails.Default
            : new LeaseFeeNoticePolicyDetails(policy.Schedule, policy.CurePeriod, policy.LetterSignOff, false);
    }
}
