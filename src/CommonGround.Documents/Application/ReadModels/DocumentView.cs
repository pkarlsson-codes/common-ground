namespace CommonGround.Documents.Application.ReadModels;

/// <summary>Detail read model for a document.</summary>
public class DocumentView
{
    /// <summary>Gets or sets the document ID.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the sensitivity display name.</summary>
    public string Sensitivity { get; set; } = string.Empty;

    /// <summary>Gets or sets the retention display name.</summary>
    public string Retention { get; set; } = string.Empty;

    /// <summary>Gets or sets the current version ID.</summary>
    public string CurrentVersionId { get; set; } = string.Empty;

    /// <summary>Gets or sets the current version's filename.</summary>
    public string CurrentFileName { get; set; } = string.Empty;

    /// <summary>Gets or sets the current version's content type.</summary>
    public string CurrentContentType { get; set; } = string.Empty;

    /// <summary>Gets or sets the current version's size in bytes.</summary>
    public long CurrentSizeBytes { get; set; }

    /// <summary>Gets or sets the user who uploaded the original version.</summary>
    public string UploadedBy { get; set; } = string.Empty;

    /// <summary>Gets or sets when the original version was uploaded.</summary>
    public DateTimeOffset UploadedAt { get; set; }

    /// <summary>Gets or sets a value indicating whether this document is soft-deleted.</summary>
    public bool IsDeleted { get; set; }

    /// <summary>Gets or sets the optional expiry date for this document.</summary>
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>Gets the lowercased, deduplicated tag list.</summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>Gets the version snapshots, oldest first.</summary>
    public List<DocumentVersionSnapshot> Versions { get; set; } = [];

    /// <summary>Gets the current attachment list.</summary>
    public List<DocumentAttachmentSnapshot> Attachments { get; set; } = [];
}

/// <summary>Read-model snapshot of a single version.</summary>
public class DocumentVersionSnapshot
{
    /// <summary>Gets or sets the version ID.</summary>
    public string VersionId { get; set; } = string.Empty;

    /// <summary>Gets or sets the version number.</summary>
    public int VersionNumber { get; set; }

    /// <summary>Gets or sets the blob storage key.</summary>
    public string BlobKey { get; set; } = string.Empty;

    /// <summary>Gets or sets the filename.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Gets or sets the content type.</summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>Gets or sets the size in bytes.</summary>
    public long SizeBytes { get; set; }

    /// <summary>Gets or sets the SHA256 hash.</summary>
    public string Sha256 { get; set; } = string.Empty;

    /// <summary>Gets or sets when this version was uploaded.</summary>
    public DateTimeOffset UploadedAt { get; set; }
}

/// <summary>Read-model snapshot of a single attachment.</summary>
public class DocumentAttachmentSnapshot
{
    /// <summary>Gets or sets the entity type.</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>Gets or sets the entity ID.</summary>
    public Guid EntityId { get; set; }

    /// <summary>Gets or sets the optional role.</summary>
    public string? Role { get; set; }

    /// <summary>Gets or sets when this attachment was created.</summary>
    public DateTimeOffset AttachedAt { get; set; }
}
