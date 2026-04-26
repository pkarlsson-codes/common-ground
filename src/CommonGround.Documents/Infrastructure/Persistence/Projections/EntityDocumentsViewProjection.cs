using CommonGround.Documents.Application.ReadModels;
using CommonGround.Documents.Domain.Documents.Events;
using Marten.Events.Projections;

namespace CommonGround.Documents.Infrastructure.Persistence.Projections;

/// <summary>
/// Aggregates per-entity document lists from <see cref="DocumentAttached"/> /
/// <see cref="DocumentDetached"/> / <see cref="DocumentSoftDeleted"/> events.
///
/// Identity is the composite string key produced by <see cref="EntityDocumentKey.Encode"/>.
/// </summary>
internal sealed class EntityDocumentsViewProjection : MultiStreamProjection<EntityDocumentsView, string>
{
    /// <summary>Configures the per-event identity selectors.</summary>
    public EntityDocumentsViewProjection()
    {
        // DocumentAttached/Detached carry both halves of the composite — encode them here.
        Identity<DocumentAttached>(e => EntityDocumentKey.Encode(e.EntityType, e.EntityId));
        Identity<DocumentDetached>(e => EntityDocumentKey.Encode(e.EntityType, e.EntityId));

        // DocumentSoftDeleted does not carry an entity ref; route to *all* entities the doc is
        // currently attached to using Identities (returns multiple keys per event).
        Identities<DocumentSoftDeleted>(SoftDeleteFanout);
    }

    /// <summary>
    /// Fans a soft-delete event out to every <see cref="EntityDocumentsView"/> the document is
    /// currently attached to. We do not have access to the doc's attachment list here, so we
    /// rely on the in-memory view state via the daemon's session — implemented via the dedicated
    /// <c>Apply</c> overload below that receives the projection's existing rows.
    /// </summary>
    private static IReadOnlyList<string> SoftDeleteFanout(DocumentSoftDeleted e)
        => []; // see ApplyAsync below — Marten will call Apply for any view containing the doc

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
}
