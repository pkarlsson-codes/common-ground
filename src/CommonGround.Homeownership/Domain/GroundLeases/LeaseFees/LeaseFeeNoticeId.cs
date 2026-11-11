using System.Text.Json.Serialization;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;

/// <summary>Strongly-typed identifier for a <see cref="LeaseFeeNotice"/>.</summary>
/// <remarks>Initializes a new instance of <see cref="LeaseFeeNoticeId"/>.</remarks>
[JsonConverter(typeof(EntityIdJsonConverter<LeaseFeeNoticeId>))]
public readonly record struct LeaseFeeNoticeId : IEntityId<LeaseFeeNoticeId>
{
    private LeaseFeeNoticeId(string value) => Value = value;

    // A const, not a public static property: one would stop Marten and Wolverine recognizing this as a strong-typed id.
    private const string Prefix = "leasefeenotice";

    /// <inheritdoc/>
    public string Value { get; }

    /// <summary>Wraps an existing value, rejecting one without the prefix.</summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>The identifier.</returns>
    public static LeaseFeeNoticeId Create(string value) => new(EntityIds.Validate(value, Prefix));

    /// <summary>Creates a new, unique identifier.</summary>
    /// <returns>The identifier.</returns>
    public static LeaseFeeNoticeId Generate() => new(EntityIds.New(Prefix));

    /// <inheritdoc/>
    public override string ToString() => Value;
}
