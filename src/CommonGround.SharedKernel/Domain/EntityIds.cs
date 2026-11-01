namespace CommonGround.SharedKernel.Domain;

/// <summary>Validation and generation shared by every <see cref="IEntityId{TSelf}"/>.</summary>
public static class EntityIds
{
    /// <summary>Returns <paramref name="value"/> when it is non-blank and starts with <paramref name="prefix"/> and a hyphen.</summary>
    /// <param name="value">The candidate value.</param>
    /// <param name="prefix">The identifier's prefix.</param>
    /// <returns><paramref name="value"/>, unchanged.</returns>
    /// <exception cref="ArgumentException">The value is blank or lacks the prefix.</exception>
    public static string Validate(string value, string prefix)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("ID value cannot be null or empty.", nameof(value));
        }

        if (!value.StartsWith(prefix + "-", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"ID value must start with prefix '{prefix}-'.", nameof(value));
        }

        return value;
    }

    /// <summary>Creates a new value: the prefix, a hyphen, and a lower-case ULID.</summary>
    /// <param name="prefix">The identifier's prefix.</param>
    /// <returns>The new value.</returns>
    public static string New(string prefix) => $"{prefix}-{NUlid.Ulid.NewUlid().ToString().ToLowerInvariant()}";

    /// <summary>Wraps <paramref name="value"/>, or returns <see langword="null"/> when it is <see langword="null"/>.</summary>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <param name="value">The value to wrap, if any.</param>
    /// <returns>The identifier, or <see langword="null"/>.</returns>
    public static TId? CreateOrNull<TId>(string? value)
        where TId : struct, IEntityId<TId> =>
        value is null ? null : TId.Create(value);
}
