using System.Globalization;
using System.Text.Json.Serialization;
using CommonGround.SharedKernel.Domain.Errors;
using CommonGround.SharedKernel.Domain.Exceptions;

namespace CommonGround.SharedKernel.Domain;

/// <summary>
/// An amount of US dollars in whole cents: a fee, a payment, a balance, a price. It may be negative, for a credit;
/// whoever takes one in says whether it must be positive. Adding, subtracting and multiplying by a count stay in
/// whole cents. Anything worked out at a finer grain, such as interest or a share of an amount, is rounded to the
/// cent once, half a cent away from zero, by <see cref="RoundedFrom"/> or <see cref="Share"/>.
/// </summary>
/// <remarks>Stored and sent as a plain JSON number, so an event holding one reads as <c>"Amount": 60.00</c>.</remarks>
[JsonConverter(typeof(MoneyJsonConverter))]
public sealed class Money : ValueObject, IComparable<Money>
{
    private const int CentDigits = 2;

    private static readonly CultureInfo UnitedStates = CultureInfo.GetCultureInfo("en-US");

    private Money(decimal amount)
    {
        Amount = amount;
    }

    /// <summary>Gets no money.</summary>
    public static Money Zero { get; } = new(0m);

    /// <summary>Gets the amount in dollars, never finer than a cent.</summary>
    public decimal Amount { get; }

    /// <summary>Gets whether the amount is zero.</summary>
    public bool IsZero => Amount == 0;

    /// <summary>Gets whether the amount is above zero.</summary>
    public bool IsPositive => Amount > 0;

    /// <summary>Gets whether the amount is below zero.</summary>
    public bool IsNegative => Amount < 0;

    /// <summary>Gets whether two amounts are the same.</summary>
    /// <param name="a">One amount.</param>
    /// <param name="b">The other amount.</param>
    /// <returns>True when both are null or the amounts are equal.</returns>
    public static bool operator ==(Money? a, Money? b) => a is null ? b is null : a.Equals(b);

    /// <summary>Gets whether two amounts differ.</summary>
    /// <param name="a">One amount.</param>
    /// <param name="b">The other amount.</param>
    /// <returns>True when the amounts are not equal.</returns>
    public static bool operator !=(Money? a, Money? b) => !(a == b);

    /// <summary>Adds two amounts.</summary>
    /// <param name="a">One amount.</param>
    /// <param name="b">The other amount.</param>
    /// <returns>The total.</returns>
    public static Money operator +(Money a, Money b) => new(Of(a) + Of(b));

    /// <summary>Subtracts one amount from another.</summary>
    /// <param name="a">The amount to subtract from.</param>
    /// <param name="b">The amount to subtract.</param>
    /// <returns>The difference.</returns>
    public static Money operator -(Money a, Money b) => new(Of(a) - Of(b));

    /// <summary>Negates an amount.</summary>
    /// <param name="a">The amount.</param>
    /// <returns>The amount with its sign flipped.</returns>
    public static Money operator -(Money a) => new(-Of(a));

    /// <summary>Multiplies an amount by a count, such as a monthly fee by the months charged.</summary>
    /// <param name="a">The amount.</param>
    /// <param name="count">How many of it.</param>
    /// <returns>The total.</returns>
    public static Money operator *(Money a, int count) => new(Of(a) * count);

    /// <summary>Multiplies an amount by a count, such as a monthly fee by the months charged.</summary>
    /// <param name="count">How many of it.</param>
    /// <param name="a">The amount.</param>
    /// <returns>The total.</returns>
    public static Money operator *(int count, Money a) => a * count;

    /// <summary>Gets how many times one amount goes into another, such as how many monthly fees a total pays.</summary>
    /// <param name="a">The amount to divide.</param>
    /// <param name="b">The amount to divide by; not zero.</param>
    /// <returns>The ratio, not rounded.</returns>
    public static decimal operator /(Money a, Money b) => Of(a) / Of(b);

    /// <summary>Gets whether one amount is less than another.</summary>
    /// <param name="a">One amount.</param>
    /// <param name="b">The other amount.</param>
    /// <returns>True when <paramref name="a"/> is less.</returns>
    public static bool operator <(Money a, Money b) => Of(a) < Of(b);

    /// <summary>Gets whether one amount is more than another.</summary>
    /// <param name="a">One amount.</param>
    /// <param name="b">The other amount.</param>
    /// <returns>True when <paramref name="a"/> is more.</returns>
    public static bool operator >(Money a, Money b) => Of(a) > Of(b);

    /// <summary>Gets whether one amount is at most another.</summary>
    /// <param name="a">One amount.</param>
    /// <param name="b">The other amount.</param>
    /// <returns>True when <paramref name="a"/> is less or the same.</returns>
    public static bool operator <=(Money a, Money b) => Of(a) <= Of(b);

    /// <summary>Gets whether one amount is at least another.</summary>
    /// <param name="a">One amount.</param>
    /// <param name="b">The other amount.</param>
    /// <returns>True when <paramref name="a"/> is more or the same.</returns>
    public static bool operator >=(Money a, Money b) => Of(a) >= Of(b);

    /// <summary>Creates an amount someone entered or stored. Refused when it has a fraction of a cent.</summary>
    /// <param name="amount">The amount in dollars.</param>
    /// <returns>The amount.</returns>
    /// <exception cref="BusinessRuleViolationException">The amount has a fraction of a cent.</exception>
    public static Money Create(decimal amount)
    {
        if (decimal.Round(amount, CentDigits) != amount)
        {
            throw BusinessRuleViolation.Money.FractionOfCent(amount);
        }

        return new Money(amount);
    }

    /// <summary>Creates an amount from one worked out at a finer grain, rounded to the cent, half a cent away from zero.</summary>
    /// <param name="amount">The worked-out amount in dollars.</param>
    /// <returns>The amount rounded to the cent.</returns>
    public static Money RoundedFrom(decimal amount) =>
        new(decimal.Round(amount, CentDigits, MidpointRounding.AwayFromZero));

    /// <summary>Gets the larger of two amounts.</summary>
    /// <param name="a">One amount.</param>
    /// <param name="b">The other amount.</param>
    /// <returns>The larger amount.</returns>
    public static Money Max(Money a, Money b) => a >= b ? a : b;

    /// <summary>Gets the smaller of two amounts.</summary>
    /// <param name="a">One amount.</param>
    /// <param name="b">The other amount.</param>
    /// <returns>The smaller amount.</returns>
    public static Money Min(Money a, Money b) => a <= b ? a : b;

    /// <summary>Gets a share of the amount, such as two-thirds of what is owed, rounded to the cent.</summary>
    /// <param name="share">The share, as a fraction: 0.5 is half.</param>
    /// <returns>The share rounded to the cent, half a cent away from zero.</returns>
    public Money Share(decimal share) => RoundedFrom(Amount * share);

    /// <inheritdoc/>
    public int CompareTo(Money? other) => other is null ? 1 : Amount.CompareTo(other.Amount);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => base.Equals(obj);

    /// <inheritdoc/>
    public override int GetHashCode() => base.GetHashCode();

    /// <summary>Returns the amount as US dollars, such as <c>$1,234.50</c> or <c>-$5.00</c>.</summary>
    /// <returns>The formatted amount.</returns>
    public override string ToString() => Amount.ToString("C2", UnitedStates);

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
    }

    private static decimal Of(Money money)
    {
        ArgumentNullException.ThrowIfNull(money);
        return money.Amount;
    }
}
