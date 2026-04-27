using CommonGround.Documents.Application.ReadModels;
using CommonGround.SharedKernel.Infrastructure.API;
using CommonGround.SharedKernel.Infrastructure.Querying;
using Marten;

namespace CommonGround.Documents.Application.Queries;

/// <summary>Lists documents in the library, paginated with optional filters.</summary>
/// <param name="Page">Pagination spec.</param>
/// <param name="IncludeDeleted">When true, includes soft-deleted documents in the result.</param>
/// <param name="ContentType">Optional MIME content-type filter (exact match).</param>
/// <param name="Tag">Optional tag filter (matches normalised lowercase tag).</param>
/// <param name="ExpiringBefore">When provided, restricts results to documents whose <c>ExpiresAt</c> is before this date.</param>
public sealed record ListDocuments(
    PageSpec Page,
    bool IncludeDeleted = false,
    string? ContentType = null,
    string? Tag = null,
    DateTimeOffset? ExpiringBefore = null);

/// <summary>Handles <see cref="ListDocuments"/>.</summary>
public static class ListDocumentsHandler
{
    /// <summary>Executes the list query against the document library projection.</summary>
    public static async Task<PaginatedResponse<DocumentLibraryItemView>> Handle(
        ListDocuments request,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var query = session.Query<DocumentLibraryItemView>().AsQueryable();

        if (!request.IncludeDeleted)
        {
            query = query.Where(d => !d.IsDeleted);
        }

        if (!string.IsNullOrWhiteSpace(request.ContentType))
        {
            var ct = request.ContentType;
            query = query.Where(d => d.CurrentContentType == ct);
        }

        if (!string.IsNullOrWhiteSpace(request.Tag))
        {
            var tag = request.Tag.Trim().ToLowerInvariant();
            query = query.Where(d => d.Tags.Contains(tag));
        }

        if (request.ExpiringBefore.HasValue)
        {
            var cutoff = request.ExpiringBefore.Value;
            query = query.Where(d => d.ExpiresAt != null && d.ExpiresAt < cutoff);
        }

        var totalCount = await query.LongCountAsync(cancellationToken).ConfigureAwait(false);

        var items = await query
            .OrderByDescending(d => d.LastModifiedAt)
            .Skip((request.Page.Number - 1) * request.Page.Size)
            .Take(request.Page.Size)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return new PaginatedResponse<DocumentLibraryItemView>
        {
            Items = [.. items],
            TotalCount = totalCount,
            PageSize = request.Page.Size,
            PageNumber = request.Page.Number,
            TotalPages = (long)Math.Ceiling(totalCount / (double)request.Page.Size),
        };
    }
}
