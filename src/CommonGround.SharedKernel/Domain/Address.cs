using System.Collections.Frozen;
using Wolverine.Postgresql.Transport;

namespace CommonGround.SharedKernel.Domain;

/// <summary>A postal address.</summary>
public class PostalAddress : ValueObject
{
    private static readonly FrozenSet<string> IsoCountryCodes = FrozenSet.ToFrozenSet(
    [
        "AD", "AE", "AF", "AG", "AI", "AL", "AM", "AO", "AQ", "AR", "AS", "AT", "AU", "AW", "AX", "AZ",
        "BA", "BB", "BD", "BE", "BF", "BG", "BH", "BI", "BJ", "BL", "BM", "BN", "BO", "BQ", "BR", "BS",
        "BT", "BV", "BW", "BY", "BZ", "CA", "CC", "CD", "CF", "CG", "CH", "CI", "CK", "CL", "CM", "CN",
        "CO", "CR", "CU", "CV", "CW", "CX", "CY", "CZ", "DE", "DJ", "DK", "DM", "DO", "DZ", "EC", "EE",
        "EG", "EH", "ER", "ES", "ET", "FI", "FJ", "FK", "FM", "FO", "FR", "GA", "GB", "GD", "GE", "GF",
        "GG", "GH", "GI", "GL", "GM", "GN", "GP", "GQ", "GR", "GS", "GT", "GU", "GW", "GY", "HK", "HM",
        "HN", "HR", "HT", "HU", "ID", "IE", "IL", "IM", "IN", "IO", "IQ", "IR", "IS", "IT", "JE", "JM",
        "JO", "JP", "KE", "KG", "KH", "KI", "KM", "KN", "KP", "KR", "KW", "KY", "KZ", "LA", "LB", "LC",
        "LI", "LK", "LR", "LS", "LT", "LU", "LV", "LY", "MA", "MC", "MD", "ME", "MF", "MG", "MH", "MK",
        "ML", "MM", "MN", "MO", "MP", "MQ", "MR", "MS", "MT", "MU", "MV", "MW", "MX", "MY", "MZ", "NA",
        "NC", "NE", "NF", "NG", "NI", "NL", "NO", "NP", "NR", "NU", "NZ", "OM", "PA", "PE", "PF", "PG",
        "PH", "PK", "PL", "PM", "PN", "PR", "PS", "PT", "PW", "PY", "QA", "RE", "RO", "RS", "RU", "RW",
        "SA", "SB", "SC", "SD", "SE", "SG", "SH", "SI", "SJ", "SK", "SL", "SM", "SN", "SO", "SR", "SS",
        "ST", "SV", "SX", "SY", "SZ", "TC", "TD", "TF", "TG", "TH", "TJ", "TK", "TL", "TM", "TN", "TO",
        "TR", "TT", "TV", "TW", "TZ", "UA", "UG", "UM", "US", "UY", "UZ", "VA", "VC", "VE", "VG", "VI",
        "VN", "VU", "WF", "WS", "YE", "YT", "ZA", "ZM", "ZW",
    ], StringComparer.Ordinal);

    private PostalAddress(string line1, string? line2, string locality, string? region, string? postalCode, string countryCode)
    {
        Line1 = line1;
        Line2 = line2;
        Locality = locality;
        Region = region;
        PostalCode = postalCode;
        CountryCode = countryCode;
    }

    /// <summary>Line 1 of the address.</summary>
    public string Line1 { get; init; }

    /// <summary>Line 2 of the address.</summary>
    public string? Line2 { get; init; }

    /// <summary>Locality of the address.</summary>
    public string Locality { get; init; }

    /// <summary>Region of the address.</summary>
    public string? Region { get; init; }

    /// <summary>Postal code of the address.</summary>
    public string? PostalCode { get; init; }

    /// <summary>Country code of the address.</summary>
    public string CountryCode { get; init; }

    /// <summary>
    /// Creates a new address.
    /// </summary>
    /// <param name="line1">First address line.</param>
    /// <param name="line2">Second address line.</param>
    /// <param name="locality">Locality.</param>
    /// <param name="region">Optional region.</param>
    /// <param name="postalCode">Optional postal code.</param>
    /// <param name="countryCode">Country code.</param>
    /// <returns>The created address.</returns>
    /// <exception cref="ArgumentException">Arguments failed validation.</exception>
    public static PostalAddress Create(
        string line1, string? line2,
        string locality, string? region,
        string? postalCode, string countryCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(line1);
        ArgumentException.ThrowIfNullOrWhiteSpace(locality);
        ArgumentException.ThrowIfNullOrWhiteSpace(countryCode);

        var normalizedCountryCode = countryCode.Trim().ToUpperInvariant();
        if (normalizedCountryCode.Length != 2 || !IsoCountryCodes.Contains(normalizedCountryCode))
        {
            throw new ArgumentException($"Unknown ISO 3166-1 country code: '{countryCode}'");
        }

        return new PostalAddress(line1, line2, locality, region, postalCode, normalizedCountryCode);
    }

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Line1;
        yield return Line2 ?? string.Empty;
        yield return Locality;
        yield return Region ?? string.Empty;
        yield return PostalCode ?? string.Empty;
        yield return CountryCode;
    }
}