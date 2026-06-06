using CommonGround.SharedKernel.Domain;

namespace CommonGround.Documents.Domain.Documents;

/// <summary>
/// Polymorphic reference linking a Document to an entity owned by another bounded context.
/// EntityType is an opaque string ("Building", "LandParcel", "GroundLease", etc.) — Documents BC never
/// resolves it to a concrete type.
/// </summary>
public sealed class DocumentAttachment : ValueObject
{
    /// <summary>Initializes a new attachment.</summary>
    public DocumentAttachment(string entityType, string entityId, string? role, UserId attachedBy, DateTimeOffset attachedAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
        ArgumentNullException.ThrowIfNull(attachedBy);

        EntityType = entityType;
        EntityId = entityId;
        Role = role;
        AttachedBy = attachedBy;
        AttachedAt = attachedAt;
    }

    /// <summary>Gets the opaque entity type (e.g. "Building").</summary>
    public string EntityType { get; }

    /// <summary>Gets the entity's string identifier.</summary>
    public string EntityId { get; }

    /// <summary>Gets the optional role (e.g. "primary", "amendment-of").</summary>
    public string? Role { get; }

    /// <summary>Gets the user who created this attachment.</summary>
    public UserId AttachedBy { get; }

    /// <summary>Gets when the attachment was created.</summary>
    public DateTimeOffset AttachedAt { get; }

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return EntityType;
        yield return EntityId;
        yield return Role ?? string.Empty;
    }
}
