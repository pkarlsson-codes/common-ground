using CommonGround.Documents.Application.ReadModels;
using CommonGround.Documents.Domain.Documents.Events;
using JasperFx.Events;
using Marten.Events.Aggregation;

namespace CommonGround.Documents.Infrastructure.Persistence.Projections;

/// <summary>Builds <see cref="DocumentView"/> from Document events.</summary>
internal sealed class DocumentViewProjection : SingleStreamProjection<DocumentView, string>
{
    /// <summary>Creates the initial view from a DocumentUploaded event.</summary>
    public static DocumentView Create(IEvent<DocumentUploaded> e) =>
        new()
        {
            Id = e.Data.AggregateId,
            Title = e.Data.Title,
            Sensitivity = e.Data.Sensitivity,
            Retention = e.Data.Retention,
            CurrentVersionId = e.Data.VersionId,
            CurrentFileName = e.Data.FileName,
            CurrentContentType = e.Data.ContentType,
            CurrentSizeBytes = e.Data.SizeBytes,
            UploadedBy = e.Data.UploadedBy,
            UploadedAt = e.Data.OccurredAt,
            IsDeleted = false,
            Versions =
            [
                new()
                {
                    VersionId = e.Data.VersionId,
                    VersionNumber = 1,
                    BlobKey = e.Data.BlobKey,
                    FileName = e.Data.FileName,
                    ContentType = e.Data.ContentType,
                    SizeBytes = e.Data.SizeBytes,
                    Sha256 = e.Data.Sha256,
                    UploadedAt = e.Data.OccurredAt,
                },
            ],
            Attachments = [],
        };

    /// <summary>Adds a new version and advances the current pointer.</summary>
    public static void Apply(DocumentView view, DocumentVersionAdded e)
    {
        view.Versions.Add(new DocumentVersionSnapshot
        {
            VersionId = e.VersionId,
            VersionNumber = e.VersionNumber,
            BlobKey = e.BlobKey,
            FileName = e.FileName,
            ContentType = e.ContentType,
            SizeBytes = e.SizeBytes,
            Sha256 = e.Sha256,
            UploadedAt = e.OccurredAt,
        });
        view.CurrentVersionId = e.VersionId;
        view.CurrentFileName = e.FileName;
        view.CurrentContentType = e.ContentType;
        view.CurrentSizeBytes = e.SizeBytes;
    }

    /// <summary>Adds an attachment to the current set.</summary>
    public static void Apply(DocumentView view, DocumentAttached e)
    {
        view.Attachments.Add(new DocumentAttachmentSnapshot
        {
            EntityType = e.EntityType,
            EntityId = e.EntityId,
            Role = e.Role,
            AttachedAt = e.OccurredAt,
        });
    }

    /// <summary>Removes an attachment from the current set.</summary>
    public static void Apply(DocumentView view, DocumentDetached e)
    {
        view.Attachments.RemoveAll(a =>
            a.EntityType == e.EntityType && a.EntityId == e.EntityId && a.Role == e.Role);
    }

    /// <summary>Marks the view as deleted.</summary>
    public static void Apply(DocumentView view, DocumentSoftDeleted _)
    {
        view.IsDeleted = true;
    }
}
