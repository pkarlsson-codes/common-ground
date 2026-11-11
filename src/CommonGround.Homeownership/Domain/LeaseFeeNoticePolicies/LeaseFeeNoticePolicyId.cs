using System.Text.Json.Serialization;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.LeaseFeeNoticePolicies;

/// <summary>Strongly-typed identifier for a <see cref="LeaseFeeNoticePolicy"/>.</summary>
/// <remarks>Initializes a new instance of <see cref="LeaseFeeNoticePolicyId"/>.</remarks>
[JsonConverter(typeof(EntityIdJsonConverter<LeaseFeeNoticePolicyId>))]
public readonly record struct LeaseFeeNoticePolicyId : IEntityId<LeaseFeeNoticePolicyId>
{
    private LeaseFeeNoticePolicyId(string value) => Value = value;

    // A const, not a public static property: one would stop Marten and Wolverine recognizing this as a strong-typed id.
    private const string Prefix = "leasefeenoticepolicy";

    /// <inheritdoc/>
    public string Value { get; }

    /// <summary>Wraps an existing value, rejecting one without the prefix.</summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>The identifier.</returns>
    public static LeaseFeeNoticePolicyId Create(string value) => new(EntityIds.Validate(value, Prefix));

    /// <summary>Creates a new, unique identifier. Use <see cref="ForTrust"/> for a trust's policy.</summary>
    /// <returns>The identifier.</returns>
    public static LeaseFeeNoticePolicyId Generate() => new(EntityIds.New(Prefix));

    /// <summary>Gets the ID of a trust's policy: one per trust, so the ID is the trust's.</summary>
    /// <param name="trustId">The trust (tenant) ID.</param>
    /// <returns>The identifier.</returns>
    public static LeaseFeeNoticePolicyId ForTrust(string trustId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(trustId);
        return new($"{Prefix}-{trustId}");
    }

    /// <inheritdoc/>
    public override string ToString() => Value;
}
