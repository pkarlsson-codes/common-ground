using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace CommonGround.SharedKernel.Infrastructure.Seeding;

/// <summary>
/// A date in seed JSON: either a fixed ISO date ("2021-09-15") or one relative to the day seeding
/// runs ("today", "today-30d", "today+2w", "today-6m"), so seeded data doesn't go stale.
/// </summary>
[JsonConverter(typeof(SeedDateJsonConverter))]
public readonly partial record struct SeedDate
{
    private readonly DateOnly? _fixed;
    private readonly int _offset;
    private readonly char _unit;

    private SeedDate(DateOnly? fixedDate, int offset, char unit)
    {
        _fixed = fixedDate;
        _offset = offset;
        _unit = unit;
    }

    /// <summary>Parses an ISO date or a relative token.</summary>
    /// <param name="value">The value from seed JSON.</param>
    /// <returns>The seed date.</returns>
    /// <exception cref="FormatException">The value is neither an ISO date nor a relative token.</exception>
    public static SeedDate Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fixedDate))
        {
            return new SeedDate(fixedDate, 0, 'd');
        }

        var match = RelativePattern().Match(value);
        if (!match.Success)
        {
            throw new FormatException(
                $"Seed date '{value}' is neither an ISO date (yyyy-MM-dd) nor a relative date such as 'today', 'today-30d', 'today+2w' or 'today-6m'.");
        }

        if (!match.Groups["sign"].Success)
        {
            return new SeedDate(null, 0, 'd');
        }

        var amount = int.Parse(match.Groups["amount"].Value, CultureInfo.InvariantCulture);
        var offset = match.Groups["sign"].Value == "-" ? -amount : amount;
        return new SeedDate(null, offset, match.Groups["unit"].Value[0]);
    }

    /// <summary>Resolves the date against the day seeding runs.</summary>
    /// <param name="today">Today in the trust's time zone.</param>
    /// <returns>The calendar date.</returns>
    public DateOnly Resolve(DateOnly today) => _fixed ?? _unit switch
    {
        'w' => today.AddDays(_offset * 7),
        'm' => today.AddMonths(_offset),
        _ => today.AddDays(_offset),
    };

    [GeneratedRegex(@"^today(?:(?<sign>[+-])(?<amount>\d+)(?<unit>[dwm]))?$", RegexOptions.CultureInvariant)]
    private static partial Regex RelativePattern();
}

/// <summary>Reads a <see cref="SeedDate"/> from a JSON string.</summary>
public sealed class SeedDateJsonConverter : JsonConverter<SeedDate>
{
    /// <inheritdoc/>
    public override SeedDate Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Expected a seed date string but found {reader.TokenType}.");
        }

        try
        {
            return SeedDate.Parse(reader.GetString()!);
        }
        catch (FormatException ex)
        {
            throw new JsonException(ex.Message, ex);
        }
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, SeedDate value, JsonSerializerOptions options)
        => throw new NotSupportedException("Seed dates are only read from seed JSON.");
}
