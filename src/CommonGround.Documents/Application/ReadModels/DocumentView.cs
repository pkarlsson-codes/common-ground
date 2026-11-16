namespace CommonGround.Documents.Application.ReadModels;

public class DocumentView
{
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the sensitivity display name.</summary>
    public string Sensitivity { get; set; } = string.Empty;

    /// <summary>Gets or sets the retention display name.</summary>
    public string Retention { get; set; } = string.Empty;

    public string CurrentVersionId { get; set; } = string.Empty;

    public string CurrentFileName { get; set; } = string.Empty;

    public string CurrentContentType { get; set; } = string.Empty;

    public long CurrentSizeBytes { get; set; }

    /// <summary>Gets or sets the user who uploaded the original version.</summary>
    public string UploadedBy { get; set; } = string.Empty;

    /// <summary>Gets or sets when the original version was uploaded.</summary>
    public DateTimeOffset UploadedAt { get; set; }

    /// <summary>Gets or sets a value indicating whether this document is soft-deleted.</summary>
    public bool IsDeleted { get; set; }

    public DateOnly? ExpiresOn { get; set; }

    /// <summary>Gets or sets the lowercased, deduplicated tag list.</summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>Gets or sets the version snapshots, oldest first.</summary>
    public List<DocumentVersionSnapshot> Versions { get; set; } = [];

    public List<DocumentAttachmentSnapshot> Attachments { get; set; } = [];
}

public class DocumentVersionSnapshot
{
    public string VersionId { get; set; } = string.Empty;

    public int VersionNumber { get; set; }

    public string BlobKey { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public string Sha256 { get; set; } = string.Empty;

    public DateTimeOffset UploadedAt { get; set; }
}

public class DocumentAttachmentSnapshot
{
    public string EntityType { get; set; } = string.Empty;

    public string EntityId { get; set; } = string.Empty;

    public string? Role { get; set; }

    public DateTimeOffset AttachedAt { get; set; }
}
