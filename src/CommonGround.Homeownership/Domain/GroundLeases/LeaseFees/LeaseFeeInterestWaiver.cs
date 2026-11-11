using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;

/// <summary>
/// Late-fee interest the trust chose not to collect. Art 5.7 lets the trust require interest, not makes it; Art 5.4
/// lets it ease the fee for a household in hardship. The reason says which.
/// </summary>
public sealed class LeaseFeeInterestWaiver : Entity<LeaseFeeInterestWaiverId>
{
    private LeaseFeeInterestWaiver(LeaseFeeInterestWaiverId id, DateOnly waivedOn, Money amount, string reason)
    {
        Id = id;
        WaivedOn = waivedOn;
        Amount = amount;
        Reason = reason;
    }

    /// <summary>Creates a waiver from its recorded facts. The lease checks the rules before deciding on it.</summary>
    /// <param name="id">The waiver's ID.</param>
    /// <param name="waivedOn">The day it was waived.</param>
    /// <param name="amount">How much interest was waived.</param>
    /// <param name="reason">Why.</param>
    public static LeaseFeeInterestWaiver Create(LeaseFeeInterestWaiverId id, DateOnly waivedOn, Money amount, string reason) =>
        new(id, waivedOn, amount, reason);

    /// <summary>Gets the day it was waived.</summary>
    public DateOnly WaivedOn { get; }

    /// <summary>Gets how much interest was waived.</summary>
    public Money Amount { get; }

    /// <summary>Gets why it was waived.</summary>
    public string Reason { get; }
}
