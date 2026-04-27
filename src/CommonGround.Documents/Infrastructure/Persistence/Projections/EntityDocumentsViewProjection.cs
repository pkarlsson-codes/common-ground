using CommonGround.Documents.Application.ReadModels;
using CommonGround.Documents.Domain.Documents.Events;
using Marten.Events.Projections;

namespace CommonGround.Documents.Infrastructure.Persistence.Projections;

/// <summary>
/// Aggregates per-entity document lists from <see cref="DocumentAttached"/> /
/// <see cref="DocumentDetached"/> / <see cref="DocumentSoftDeletedForAttachment"/> events.
///
/// Identity is the composite string key produced by <see cref="EntityDocumentKey.Encode"/>.
/// </summary>
internal sealed class EntityDocumentsViewProjection : MultiStreamProjection<EntityDocumentsView, string>
{
    /// <summary>Configures the per-event identity selectors.</summary>
    public EntityDocumentsViewProjection()
    {
        Identity<DocumentAttached>(e => EntityDocumentKey.Encode(e.EntityType, e.EntityId));
        Identity<DocumentDetached>(e => EntityDocumentKey.Encode(e.EntityType, e.EntityId));

        // Soft-delete is fanned out by the aggregate into a per-attachment event so we can
        // route via Identity<> here without a cross-stream lookup.
        Identity<DocumentSoftDeletedForAttachment>(e => EntityDocumentKey.Encode(e.EntityType, e.EntityId));
    }

    /// <summary>Adds a new attachment row.</summary>
    public static void Apply(EntityDocumentsView view, DocumentAttached e)
    {
        view.EntityType = e.EntityType;
        view.EntityId = e.EntityId;
        view.Id = EntityDocumentKey.Encode(e.EntityType, e.EntityId);

        var existing = view.Items.FirstOrDefault(i =>
            i.DocumentId == e.AggregateId && i.Role == e.Role);
        if (existing is not null)
            return;

        view.Items.Add(new EntityDocumentAttachmentItem
        {
            DocumentId = e.AggregateId,
            Title = e.Title,
            CurrentFileName = e.CurrentFileName,
            CurrentContentType = e.CurrentContentType,
            CurrentSizeBytes = e.CurrentSizeBytes,
            Role = e.Role,
            AttachedAt = e.OccurredAt,
            IsDeleted = false,
        });
    }

    /// <summary>Removes a detached attachment row.</summary>
    public static void Apply(EntityDocumentsView view, DocumentDetached e)
    {
        view.Items.RemoveAll(i =>
            i.DocumentId == e.AggregateId && i.Role == e.Role);
    }

    /// <summary>Marks the matching attachment row as soft-deleted.</summary>
    public static void Apply(EntityDocumentsView view, DocumentSoftDeletedForAttachment e)
    {
        foreach (var item in view.Items.Where(i => i.DocumentId == e.AggregateId && i.Role == e.Role))
        {
            item.IsDeleted = true;
        }
    }
}
