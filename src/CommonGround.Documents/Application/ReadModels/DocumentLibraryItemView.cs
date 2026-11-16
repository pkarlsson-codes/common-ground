namespace CommonGround.Documents.Application.ReadModels;

/// <summary>
/// One row per document, optimised for the document library listing.
/// One-to-one with <see cref="DocumentView"/> identity.
/// </summary>
public class DocumentLibraryItemView
{
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the sensitivity display name.</summary>
    public string Sensitivity { get; set; } = string.Empty;

    /// <summary>Gets or sets the retention display name.</summary>
    public string Retention { get; set; } = string.Empty;

    public string CurrentFileName { get; set; } = string.Empty;

    public string CurrentContentType { get; set; } = string.Empty;

    public long CurrentSizeBytes { get; set; }

    public int CurrentVersionNumber { get; set; }

    public string UploadedBy { get; set; } = string.Empty;

    /// <summary>Gets or sets when the original version was uploaded.</summary>
    public DateTimeOffset UploadedAt { get; set; }

    /// <summary>Gets or sets when the current version was uploaded.</summary>
    public DateTimeOffset LastModifiedAt { get; set; }

    /// <summary>Gets or sets a value indicating whether this document is soft-deleted.</summary>
    public bool IsDeleted { get; set; }

    public DateOnly? ExpiresOn { get; set; }

    /// <summary>Gets or sets the lowercased, deduplicated tag list.</summary>
    public List<string> Tags { get; set; } = [];
}
