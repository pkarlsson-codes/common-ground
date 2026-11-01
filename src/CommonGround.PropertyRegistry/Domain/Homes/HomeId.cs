using System.Text.Json.Serialization;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.PropertyRegistry.Domain.Homes;

/// <summary>Strongly-typed identifier for a <see cref="Home"/>.</summary>
/// <remarks>Initializes a new instance of <see cref="HomeId"/>.</remarks>
[JsonConverter(typeof(EntityIdJsonConverter<HomeId>))]
public readonly record struct HomeId : IEntityId<HomeId>
{
    private HomeId(string value) => Value = value;

    /// <summary>Gets the prefix every value starts with, before a hyphen.</summary>
    public static string Prefix => "home";

    /// <inheritdoc/>
    public string Value { get; }

    /// <summary>Wraps an existing value, rejecting one without the prefix.</summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>The identifier.</returns>
    public static HomeId Create(string value) => new(EntityIds.Validate(value, Prefix));

    /// <summary>Creates a new, unique identifier.</summary>
    /// <returns>The identifier.</returns>
    public static HomeId Generate() => new(EntityIds.New(Prefix));

    /// <inheritdoc/>
    public override string ToString() => Value;
}
