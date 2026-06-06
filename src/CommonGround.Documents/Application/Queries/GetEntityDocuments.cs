using CommonGround.Documents.Application.ReadModels;
using CommonGround.SharedKernel.Infrastructure.API;
using CommonGround.SharedKernel.Infrastructure.Querying;
using Marten;

namespace CommonGround.Documents.Application.Queries;

/// <summary>Returns the paginated current attachments for a given entity.</summary>
/// <remarks>
/// Soft-deleted attachments are flagged inline by <see cref="EntityDocumentAttachmentItem.IsDeleted"/>
/// (set by the per-entity projection via <c>DocumentSoftDeletedForAttachment</c>) and excluded from
/// results unless <paramref name="IncludeDeleted"/> is true.
/// </remarks>
/// <param name="EntityType">Opaque entity type (e.g. "Building"). Case-sensitive.</param>
/// <param name="EntityId">The entity's string identifier.</param>
/// <param name="Page">Pagination spec.</param>
/// <param name="IncludeDeleted">When true, includes soft-deleted attachments in the result.</param>
public sealed record GetEntityDocuments(string EntityType, string EntityId, PageSpec Page, bool IncludeDeleted = false);

/// <summary>Handles <see cref="GetEntityDocuments"/>.</summary>
public static class GetEntityDocumentsHandler
{
    /// <summary>Handles the get entity documents request.</summary>
    public static async Task<PaginatedResponse<EntityDocumentAttachmentItem>> Handle(
        GetEntityDocuments request,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var key = EntityDocumentKey.Encode(request.EntityType, request.EntityId);

        var view = await session.LoadAsync<EntityDocumentsView>(key, cancellationToken).ConfigureAwait(false);
        if (view is null)
        {
            return new PaginatedResponse<EntityDocumentAttachmentItem>
            {
                Items = [],
                TotalCount = 0,
                PageSize = request.Page.Size,
                PageNumber = request.Page.Number,
                TotalPages = 0,
            };
        }

        var ordered = view.Items
            .Where(i => request.IncludeDeleted || !i.IsDeleted)
            .OrderByDescending(i => i.AttachedAt)
            .ToList();

        var pageItems = ordered
            .Skip((request.Page.Number - 1) * request.Page.Size)
            .Take(request.Page.Size)
            .ToArray();

        return new PaginatedResponse<EntityDocumentAttachmentItem>
        {
            Items = pageItems,
            TotalCount = ordered.Count,
            PageSize = request.Page.Size,
            PageNumber = request.Page.Number,
            TotalPages = (int)Math.Ceiling(ordered.Count / (double)request.Page.Size),
        };
    }
}
