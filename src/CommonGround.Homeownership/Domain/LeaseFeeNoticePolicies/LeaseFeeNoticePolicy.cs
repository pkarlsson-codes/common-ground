using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.Homeownership.Domain.LeaseFeeNoticePolicies.Events;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.LeaseFeeNoticePolicies;

/// <summary>
/// A trust's policy for late lease fee notices: when each is due, and how its letters are signed. One per trust; a
/// trust that never set it uses <see cref="LeaseFeeNoticeSchedule.Default"/> and no sign-off.
/// </summary>
public sealed class LeaseFeeNoticePolicy : AggregateRoot<LeaseFeeNoticePolicyId>
{
#pragma warning disable CS8618 // State is set by Create from the stream's first event.
    private LeaseFeeNoticePolicy() { }
#pragma warning restore CS8618

    /// <summary>Gets when each notice is due.</summary>
    public LeaseFeeNoticeSchedule Schedule { get; private set; }

    /// <summary>Gets the lines printed at the end of every notice letter; empty for none.</summary>
    public string LetterSignOff { get; private set; }

    /// <summary>
    /// Decides to set a trust's policy, the first time (start the stream with it) or later (append it). The sign-off
    /// is trimmed; line breaks inside it are kept.
    /// </summary>
    public static LeaseFeeNoticePolicySet Set(
        LeaseFeeNoticePolicyId id, LeaseFeeNoticeSchedule schedule, string? letterSignOff, UserId setBy)
    {
        ArgumentNullException.ThrowIfNull(schedule);

        return new LeaseFeeNoticePolicySet(
            id.Value,
            schedule.FirstNoticeDays,
            schedule.SecondNoticeDays,
            letterSignOff?.Trim() ?? string.Empty,
            setBy.Value);
    }

    /// <summary>Evolves a policy from its stream's first event.</summary>
    public static LeaseFeeNoticePolicy Create(LeaseFeeNoticePolicySet e)
    {
        ArgumentNullException.ThrowIfNull(e);

        var policy = new LeaseFeeNoticePolicy { Id = LeaseFeeNoticePolicyId.Create(e.AggregateId) };
        policy.Apply(e);
        return policy;
    }

    /// <summary>Applies <see cref="LeaseFeeNoticePolicySet"/>.</summary>
    public void Apply(LeaseFeeNoticePolicySet e)
    {
        ArgumentNullException.ThrowIfNull(e);

        Schedule = LeaseFeeNoticeSchedule.Create(e.FirstNoticeDays, e.SecondNoticeDays);
        LetterSignOff = e.LetterSignOff;
    }
}
