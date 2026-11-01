using System.Text.Json.Serialization;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.Documents.Domain.Documents;

/// <summary>Strongly-typed identifier for a Document.</summary>
[JsonConverter(typeof(EntityIdJsonConverter<DocumentId>))]
public readonly record struct DocumentId : IEntityId<DocumentId>
{
    private DocumentId(string value) => Value = value;

    /// <summary>Gets the prefix every value starts with, before a hyphen.</summary>
    public static string Prefix => "doc";

    /// <inheritdoc/>
    public string Value { get; }

    /// <summary>Wraps an existing value, rejecting one without the prefix.</summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>The identifier.</returns>
    public static DocumentId Create(string value) => new(EntityIds.Validate(value, Prefix));

    /// <summary>Creates a new, unique identifier.</summary>
    /// <returns>The identifier.</returns>
    public static DocumentId Generate() => new(EntityIds.New(Prefix));

    /// <inheritdoc/>
    public override string ToString() => Value;
}
