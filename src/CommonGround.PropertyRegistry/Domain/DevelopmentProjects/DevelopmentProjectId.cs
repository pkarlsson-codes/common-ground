using System.Text.Json.Serialization;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.PropertyRegistry.Domain.DevelopmentProjects;

/// <summary>Strongly-typed identifier for a DevelopmentProject.</summary>
/// <remarks>Initializes a new instance of <see cref="DevelopmentProjectId"/>.</remarks>
[JsonConverter(typeof(EntityIdJsonConverter<DevelopmentProjectId>))]
public readonly record struct DevelopmentProjectId : IEntityId<DevelopmentProjectId>
{
    private DevelopmentProjectId(string value) => Value = value;

    // A const, not a public static property: one would stop Marten and Wolverine recognizing this as a strong-typed id.
    private const string Prefix = "project";

    /// <inheritdoc/>
    public string Value { get; }

    /// <summary>Wraps an existing value, rejecting one without the prefix.</summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>The identifier.</returns>
    public static DevelopmentProjectId Create(string value) => new(EntityIds.Validate(value, Prefix));

    /// <summary>Creates a new, unique identifier.</summary>
    /// <returns>The identifier.</returns>
    public static DevelopmentProjectId Generate() => new(EntityIds.New(Prefix));

    /// <inheritdoc/>
    public override string ToString() => Value;
}
