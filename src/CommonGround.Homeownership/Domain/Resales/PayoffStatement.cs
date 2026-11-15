using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.Resales;

/// <summary>
/// What a seller's lease owes as of a day, for the closing agent to pay from the sale proceeds. Split from the
/// lease's own <see cref="LeaseFeeStatement"/>, so the figure always matches the lease's ledger.
/// </summary>
/// <param name="GoodThrough">The day the figures are worked out for.</param>
/// <param name="FeesOwed">Monthly fees and opening balance charged, less what was paid; zero if paid up or over.</param>
/// <param name="InterestOwed">Interest owed after waivers.</param>
/// <param name="Credit">What was paid beyond the fees charged; zero if none.</param>
/// <param name="OldestUnpaidMonth">The due date of the oldest unpaid fee, or null when none is unpaid.</param>
public sealed record PayoffStatement(
    DateOnly GoodThrough,
    Money FeesOwed,
    Money InterestOwed,
    Money Credit,
    DateOnly? OldestUnpaidMonth)
{
    /// <summary>Gets what the closing agent pays: fees and interest less credit, never below zero.</summary>
    public Money TotalOwed => Money.Max(Money.Zero, FeesOwed + InterestOwed - Credit);

    /// <summary>Splits a lease fee statement into a payoff statement.</summary>
    public static PayoffStatement From(LeaseFeeStatement statement)
    {
        ArgumentNullException.ThrowIfNull(statement);

        var net = statement.TotalCharged - statement.TotalPaid;

        return new PayoffStatement(
            statement.AsOf,
            Money.Max(Money.Zero, net),
            statement.InterestDue,
            Money.Max(Money.Zero, -net),
            statement.OldestUnpaidDueDate);
    }
}
