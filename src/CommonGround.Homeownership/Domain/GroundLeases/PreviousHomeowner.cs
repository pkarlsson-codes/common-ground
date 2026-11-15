using CommonGround.Homeownership.Domain.HeirTransfers;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeases;

/// <summary>Someone who held the home before the current homeowner, and the transfer that ended their hold.</summary>
public sealed class PreviousHomeowner : ValueObject
{
    private PreviousHomeowner(Homeowner homeowner, DateOnly transferredOn, HeirTransferId heirTransferId)
    {
        Homeowner = homeowner;
        TransferredOn = transferredOn;
        HeirTransferId = heirTransferId;
    }

    /// <summary>Creates a history entry.</summary>
    /// <param name="homeowner">The homeowner who held the home.</param>
    /// <param name="transferredOn">The day the home passed to the next homeowner.</param>
    /// <param name="heirTransferId">The heir transfer that moved the home on.</param>
    public static PreviousHomeowner Create(Homeowner homeowner, DateOnly transferredOn, HeirTransferId heirTransferId)
    {
        ArgumentNullException.ThrowIfNull(homeowner);

        return new PreviousHomeowner(homeowner, transferredOn, heirTransferId);
    }

    /// <summary>Gets the homeowner who held the home.</summary>
    public Homeowner Homeowner { get; }

    /// <summary>Gets the day the home passed to the next homeowner.</summary>
    public DateOnly TransferredOn { get; }

    /// <summary>Gets the heir transfer that moved the home on.</summary>
    public HeirTransferId HeirTransferId { get; }

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Homeowner;
        yield return TransferredOn;
        yield return HeirTransferId;
    }
}
