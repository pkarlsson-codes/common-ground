using System.Text.Json.Serialization;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.HomeBuyers.Domain.HomeSelections;

/// <summary>Strongly-typed identifier for a <see cref="HomeSelection"/>.</summary>
/// <remarks>Initializes a new instance of <see cref="HomeSelectionId"/>.</remarks>
[JsonConverter(typeof(EntityIdJsonConverter<HomeSelectionId>))]
public readonly record struct HomeSelectionId : IEntityId<HomeSelectionId>
{
    private HomeSelectionId(string value) => Value = value;

    // A const, not a public static property: one would stop Marten and Wolverine recognizing this as a strong-typed id.
    private const string Prefix = "selection";

    /// <inheritdoc/>
    public string Value { get; }

    /// <summary>Wraps an existing value, rejecting one without the prefix.</summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>The identifier.</returns>
    public static HomeSelectionId Create(string value) => new(EntityIds.Validate(value, Prefix));

    /// <summary>Creates a new, unique identifier.</summary>
    /// <returns>The identifier.</returns>
    public static HomeSelectionId Generate() => new(EntityIds.New(Prefix));

    /// <inheritdoc/>
    public override string ToString() => Value;
}
