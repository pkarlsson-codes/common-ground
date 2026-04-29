using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Domain.LeasableUnits;
using CommonGround.SharedKernel.Infrastructure.API;
using CommonGround.SharedKernel.Infrastructure.Querying;
using Marten;
using Marten.Pagination;

namespace CommonGround.PropertyRegistry.Application.Queries;

/// <summary>Returns a paginated list of assessments for a dwelling unit, newest first.</summary>
/// <param name="UnitId">The ID of the unit whose assessments to retrieve.</param>
/// <param name="Page">The pagination spec.</param>
public sealed record GetDwellingUnitAssessments(LeasableUnitId UnitId, PageSpec Page);

/// <summary>Handles <see cref="GetDwellingUnitAssessments"/>.</summary>
public static class GetDwellingUnitAssessmentsHandler
{
    /// <summary>Returns a paginated list of <see cref="DwellingUnitAssessmentView"/>, ordered newest first.</summary>
    public static async Task<PaginatedResponse<DwellingUnitAssessmentView>> Handle(
        GetDwellingUnitAssessments request,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var page = await session.Query<DwellingUnitAssessmentView>()
            .Where(x => x.UnitId == request.UnitId.Value)
            .OrderByDescending(x => x.AssessmentDate)
            .ThenByDescending(x => x.RecordedAt)
            .ToPagedListAsync(request.Page.Number, request.Page.Size, cancellationToken)
            .ConfigureAwait(false);

        return new PaginatedResponse<DwellingUnitAssessmentView>
        {
            Items = [.. page],
            TotalCount = (int)page.TotalItemCount,
            PageSize = (int)page.PageSize,
            PageNumber = (int)page.PageNumber,
            TotalPages = (int)page.PageCount,
        };
    }
}
