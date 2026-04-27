namespace CommonGround.Documents.Application.ReadModels;

/// <summary>
/// One row per document, optimised for the document library listing.
/// One-to-one with <see cref="DocumentView"/> identity.
/// </summary>
public class DocumentLibraryItemView
{
    /// <summary>Gets or sets the document ID.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the sensitivity display name.</summary>
    public string Sensitivity { get; set; } = string.Empty;

    /// <summary>Gets or sets the retention display name.</summary>
    public string Retention { get; set; } = string.Empty;

    /// <summary>Gets or sets the current version's filename.</summary>
    public string CurrentFileName { get; set; } = string.Empty;

    /// <summary>Gets or sets the current version's content type.</summary>
    public string CurrentContentType { get; set; } = string.Empty;

    /// <summary>Gets or sets the current version's size in bytes.</summary>
    public long CurrentSizeBytes { get; set; }

    /// <summary>Gets or sets the current version number.</summary>
    public int CurrentVersionNumber { get; set; }

    /// <summary>Gets or sets the user who originally uploaded this document.</summary>
    public string UploadedBy { get; set; } = string.Empty;

    /// <summary>Gets or sets when the original version was uploaded.</summary>
    public DateTimeOffset UploadedAt { get; set; }

    /// <summary>Gets or sets when the current version was uploaded.</summary>
    public DateTimeOffset LastModifiedAt { get; set; }

    /// <summary>Gets or sets a value indicating whether this document is soft-deleted.</summary>
    public bool IsDeleted { get; set; }

    /// <summary>Gets or sets the optional expiry timestamp.</summary>
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>Gets the lowercased, deduplicated tag list.</summary>
    public List<string> Tags { get; set; } = [];
}
