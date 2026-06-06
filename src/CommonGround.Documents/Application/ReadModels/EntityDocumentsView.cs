namespace CommonGround.Documents.Application.ReadModels;

/// <summary>
/// One row per <c>(EntityType, EntityId)</c> pair, listing all current attachments to that entity.
/// Identity is the encoded composite key produced by <see cref="EntityDocumentKey.Encode"/>.
/// </summary>
public class EntityDocumentsView
{
    /// <summary>Gets or sets the encoded composite key, e.g. "Building:6f0e9a1d8c2c4f5db93b6c1edc2a3f7e".</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the entity type segment of the key.</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>Gets or sets the entity id segment of the key.</summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>Gets the current attachments to this entity.</summary>
    public List<EntityDocumentAttachmentItem> Items { get; set; } = [];
}

/// <summary>One attached document on an entity, denormalised for listing.</summary>
public class EntityDocumentAttachmentItem
{
    /// <summary>Gets or sets the document ID.</summary>
    public string DocumentId { get; set; } = string.Empty;

    /// <summary>Gets or sets the document title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the current version filename.</summary>
    public string CurrentFileName { get; set; } = string.Empty;

    /// <summary>Gets or sets the current version content type.</summary>
    public string CurrentContentType { get; set; } = string.Empty;

    /// <summary>Gets or sets the current version size in bytes.</summary>
    public long CurrentSizeBytes { get; set; }

    /// <summary>Gets or sets the optional attachment role.</summary>
    public string? Role { get; set; }

    /// <summary>Gets or sets when this attachment was created.</summary>
    public DateTimeOffset AttachedAt { get; set; }

    /// <summary>Gets or sets a value indicating whether the document is soft-deleted.</summary>
    public bool IsDeleted { get; set; }
}

/// <summary>Encodes / decodes the composite identity for <see cref="EntityDocumentsView"/>.</summary>
public static class EntityDocumentKey
{
    /// <summary>Encodes a composite key as <c>"{EntityType}:{EntityId}"</c>.</summary>
    public static string Encode(string entityType, string entityId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
        ArgumentException.ThrowIfNullOrWhiteSpace(entityId);
        return $"{entityType}:{entityId}";
    }
}
