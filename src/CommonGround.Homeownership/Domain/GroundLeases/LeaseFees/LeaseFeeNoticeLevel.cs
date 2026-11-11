using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;

/// <summary>
/// Which late notice a letter is. Ch 23: a first notice within the first month, a second after it. Then the notice of
/// default (Art 12.1), which starts the cure period.
/// </summary>
public sealed class LeaseFeeNoticeLevel : Enumeration<LeaseFeeNoticeLevel>
{
    /// <summary>The first late notice, sent once the grace period has passed.</summary>
    public static readonly LeaseFeeNoticeLevel First = new(1, "First");

    /// <summary>The second late notice, sent once the fee is more than a month late; warns of a notice of default.</summary>
    public static readonly LeaseFeeNoticeLevel Second = new(2, "Second");

    /// <summary>The notice of default (Art 12.1): pay within the cure period or the trust may end the lease.</summary>
    public static readonly LeaseFeeNoticeLevel Default = new(3, "Default");

    private LeaseFeeNoticeLevel(int value, string displayName) : base(value, displayName) { }
}
