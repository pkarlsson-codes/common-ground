namespace CommonGround.SharedKernel.Domain;

/// <summary>Any strongly-typed identifier, for code that only needs its string value.</summary>
public interface IEntityId
{
    /// <summary>Gets the wrapped string value.</summary>
    string Value { get; }
}

/// <summary>
/// A strongly-typed identifier: an immutable struct wrapping a prefixed string such as
/// <c>household-01jq...</c>. Marten recognizes this shape (a single public <c>Value</c> and a static
/// builder) as a strong-typed id, so aggregates can use it as their <c>Id</c> while event streams stay
/// keyed by the plain string. Implementations must not add a public static property: Marten and
/// Wolverine then stop treating the type as a strong-typed id.
/// </summary>
/// <typeparam name="TSelf">The implementing identifier type.</typeparam>
public interface IEntityId<TSelf> : IEntityId
    where TSelf : struct, IEntityId<TSelf>
{
    /// <summary>Wraps an existing value, rejecting one without this identifier's prefix.</summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>The identifier.</returns>
    static abstract TSelf Create(string value);

    /// <summary>Creates a new, unique identifier.</summary>
    /// <returns>The identifier.</returns>
    static abstract TSelf Generate();
}
