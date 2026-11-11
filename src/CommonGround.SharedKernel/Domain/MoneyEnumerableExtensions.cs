namespace CommonGround.SharedKernel.Domain;

/// <summary>Adds up <see cref="Money"/>.</summary>
public static class MoneyEnumerableExtensions
{
    /// <summary>Adds up amounts; <see cref="Money.Zero"/> when there are none.</summary>
    /// <param name="amounts">The amounts.</param>
    /// <returns>The total.</returns>
    public static Money Sum(this IEnumerable<Money> amounts)
    {
        ArgumentNullException.ThrowIfNull(amounts);
        return amounts.Aggregate(Money.Zero, (total, amount) => total + amount);
    }

    /// <summary>Adds up an amount of each item; <see cref="Money.Zero"/> when there are none.</summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="items">The items.</param>
    /// <param name="amount">The amount of each.</param>
    /// <returns>The total.</returns>
    public static Money Sum<T>(this IEnumerable<T> items, Func<T, Money> amount)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(amount);
        return items.Select(amount).Sum();
    }
}
