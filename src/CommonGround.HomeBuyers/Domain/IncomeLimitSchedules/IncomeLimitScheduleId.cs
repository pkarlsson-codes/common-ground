using System.Text.Json.Serialization;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.HomeBuyers.Domain.IncomeLimitSchedules;

/// <summary>Strongly-typed identifier for a <see cref="IncomeLimitSchedule"/>.</summary>
/// <remarks>Initializes a new instance of <see cref="IncomeLimitScheduleId"/>.</remarks>
[JsonConverter(typeof(EntityIdJsonConverter<IncomeLimitScheduleId>))]
public readonly record struct IncomeLimitScheduleId : IEntityId<IncomeLimitScheduleId>
{
    private IncomeLimitScheduleId(string value) => Value = value;

    // A const, not a public static property: one would stop Marten and Wolverine recognizing this as a strong-typed id.
    private const string Prefix = "incomelimitschedule";

    /// <inheritdoc/>
    public string Value { get; }

    /// <summary>Wraps an existing value, rejecting one without the prefix.</summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>The identifier.</returns>
    public static IncomeLimitScheduleId Create(string value) => new(EntityIds.Validate(value, Prefix));

    /// <summary>Creates a new, unique identifier.</summary>
    /// <returns>The identifier.</returns>
    public static IncomeLimitScheduleId Generate() => new(EntityIds.New(Prefix));

    /// <inheritdoc/>
    public override string ToString() => Value;
}
