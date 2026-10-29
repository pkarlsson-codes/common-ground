using CommonGround.Documents.Application.ReadModels;
using CommonGround.Documents.Domain.Documents.Events;
using JasperFx.Events;
using Marten.Events.Aggregation;

namespace CommonGround.Documents.Infrastructure.Persistence.Projections;

/// <summary>Builds <see cref="DocumentLibraryItemView"/> rows for the document library listing.</summary>
internal sealed class DocumentLibraryItemViewProjection : SingleStreamProjection<DocumentLibraryItemView, string>
{
    /// <summary>Creates the initial library row from a DocumentUploaded event.</summary>
    public static DocumentLibraryItemView Create(IEvent<DocumentUploaded> e) =>
        new()
        {
            Id = e.Data.AggregateId,
            Title = e.Data.Title,
            Sensitivity = e.Data.Sensitivity,
            Retention = e.Data.Retention,
            CurrentFileName = e.Data.FileName,
            CurrentContentType = e.Data.ContentType,
            CurrentSizeBytes = e.Data.SizeBytes,
            CurrentVersionNumber = 1,
            UploadedBy = e.Data.UploadedBy,
            UploadedAt = e.Data.OccurredAt,
            LastModifiedAt = e.Data.OccurredAt,
            IsDeleted = false,
            ExpiresOn = null,
            Tags = [],
        };

    /// <summary>Advances current-version pointers when a new version is added.</summary>
    public static void Apply(DocumentLibraryItemView view, DocumentVersionAdded e)
    {
        view.CurrentFileName = e.FileName;
        view.CurrentContentType = e.ContentType;
        view.CurrentSizeBytes = e.SizeBytes;
        view.CurrentVersionNumber = e.VersionNumber;
        view.LastModifiedAt = e.OccurredAt;
    }

    /// <summary>Updates editable metadata fields and bumps last-modified.</summary>
    public static void Apply(DocumentLibraryItemView view, DocumentMetadataUpdated e)
    {
        view.Title = e.Title;
        view.Sensitivity = e.Sensitivity;
        view.ExpiresOn = e.ClearExpiresOn ? null : e.ExpiresOn;
        view.LastModifiedAt = e.OccurredAt;
    }

    /// <summary>Adds a tag (deduped).</summary>
    public static void Apply(DocumentLibraryItemView view, DocumentTagged e)
    {
        if (!view.Tags.Contains(e.Tag, StringComparer.Ordinal))
        {
            view.Tags.Add(e.Tag);
        }

        view.LastModifiedAt = e.OccurredAt;
    }

    /// <summary>Removes a tag.</summary>
    public static void Apply(DocumentLibraryItemView view, DocumentUntagged e)
    {
        view.Tags.RemoveAll(t => string.Equals(t, e.Tag, StringComparison.Ordinal));
        view.LastModifiedAt = e.OccurredAt;
    }

    /// <summary>Marks the row as deleted.</summary>
    public static void Apply(DocumentLibraryItemView view, DocumentSoftDeleted e)
    {
        view.IsDeleted = true;
        view.LastModifiedAt = e.OccurredAt;
    }
}
