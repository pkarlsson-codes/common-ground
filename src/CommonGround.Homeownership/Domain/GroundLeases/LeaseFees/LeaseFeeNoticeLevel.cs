using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;

/// <summary>Which late notice a letter is. Ch 23: a first notice within the first month, a second after it.</summary>
public sealed class LeaseFeeNoticeLevel : Enumeration<LeaseFeeNoticeLevel>
{
    /// <summary>The first late notice, sent once the grace period has passed.</summary>
    public static readonly LeaseFeeNoticeLevel First = new(1, "First");

    /// <summary>The second late notice, sent once the fee is more than a month late; warns of a notice of default.</summary>
    public static readonly LeaseFeeNoticeLevel Second = new(2, "Second");

    private LeaseFeeNoticeLevel(int value, string displayName) : base(value, displayName) { }
}
