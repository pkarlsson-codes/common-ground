namespace CommonGround.SharedKernel.Infrastructure.API;

/// <summary>Helper class for building query strings.</summary>
public sealed class QueryStringBuilder
{
    private readonly List<(string Key, string Value)> _params = [];

    /// <summary>Adds a key-value pair to the query string.</summary>
    /// <param name="key">The query parameter key.</param>
    /// <param name="value">The query parameter value.</param>
    /// <returns>The current <see cref="QueryStringBuilder"/> instance.</returns>
    public QueryStringBuilder Add(string key, string value)
    {
        _params.Add((Uri.EscapeDataString(key), Uri.EscapeDataString(value)));
        return this;
    }

    /// <summary>Adds a key-value pair to the query string if the value is not null or whitespace.</summary>
    /// <param name="key">The query parameter key.</param>
    /// <param name="value">The query parameter value.</param>
    /// <returns>The current <see cref="QueryStringBuilder"/> instance.</returns>
    public QueryStringBuilder Add(string key, int value)
        => Add(key, value.ToString(System.Globalization.CultureInfo.InvariantCulture));

    /// <summary>Adds a key-value pair to the query string if the value is not null or whitespace.</summary>
    /// <param name="key">The query parameter key.</param>
    /// <param name="value">The query parameter value.</param>
    /// <returns>The current <see cref="QueryStringBuilder"/> instance.</returns>
    public QueryStringBuilder AddIfNotWhiteSpace(string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            Add(key, value);
        }

        return this;
    }

    /// <summary>
    /// Builds the query string.
    /// </summary>
    /// <returns>The constructed query string.</returns>
    public string Build()
        => _params.Count == 0
            ? string.Empty
            : "?" + string.Join("&", _params.Select(p => $"{p.Key}={p.Value}"));
}
