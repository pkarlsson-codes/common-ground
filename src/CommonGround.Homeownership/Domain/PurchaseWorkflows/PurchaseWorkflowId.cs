using System.Text.Json.Serialization;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.PurchaseWorkflows;

/// <summary>Strongly-typed identifier for a purchase workflow.</summary>
[JsonConverter(typeof(EntityIdJsonConverter<PurchaseWorkflowId>))]
public readonly record struct PurchaseWorkflowId : IEntityId<PurchaseWorkflowId>
{
    private PurchaseWorkflowId(string value) => Value = value;

    // A const, not a public static property: one would stop Marten and Wolverine recognizing this as a strong-typed id.
    private const string Prefix = "purchase";

    /// <inheritdoc/>
    public string Value { get; }

    /// <summary>Wraps an existing value, rejecting one without the prefix.</summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>The identifier.</returns>
    public static PurchaseWorkflowId Create(string value) => new(EntityIds.Validate(value, Prefix));

    /// <summary>Creates a new, unique identifier.</summary>
    /// <returns>The identifier.</returns>
    public static PurchaseWorkflowId Generate() => new(EntityIds.New(Prefix));

    /// <inheritdoc/>
    public override string ToString() => Value;
}
