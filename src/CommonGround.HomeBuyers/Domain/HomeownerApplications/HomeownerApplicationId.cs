using System.Text.Json.Serialization;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.HomeBuyers.Domain.HomeownerApplications;

/// <summary>Strongly-typed identifier for a homeowner application.</summary>
[JsonConverter(typeof(EntityIdJsonConverter<HomeownerApplicationId>))]
public readonly record struct HomeownerApplicationId : IEntityId<HomeownerApplicationId>
{
    private HomeownerApplicationId(string value) => Value = value;

    /// <summary>Gets the prefix every value starts with, before a hyphen.</summary>
    public static string Prefix => "application";

    /// <inheritdoc/>
    public string Value { get; }

    /// <summary>Wraps an existing value, rejecting one without the prefix.</summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>The identifier.</returns>
    public static HomeownerApplicationId Create(string value) => new(EntityIds.Validate(value, Prefix));

    /// <summary>Creates a new, unique identifier.</summary>
    /// <returns>The identifier.</returns>
    public static HomeownerApplicationId Generate() => new(EntityIds.New(Prefix));

    /// <inheritdoc/>
    public override string ToString() => Value;
}
