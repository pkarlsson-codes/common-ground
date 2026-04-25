using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Domain.Buildings;
using CommonGround.SharedKernel.Infrastructure.API;
using CommonGround.SharedKernel.Infrastructure.Querying;
using Marten;
using Marten.Pagination;

namespace CommonGround.PropertyRegistry.Application.Queries;

/// <summary>Returns a paginated list of assessments for a building, newest first.</summary>
/// <param name="BuildingId">The ID of the building whose assessments to retrieve.</param>
/// <param name="Page">The pagination spec.</param>
public sealed record GetBuildingAssessments(BuildingId BuildingId, PageSpec Page);

/// <summary>Handles <see cref="GetBuildingAssessments"/>.</summary>
public static class GetBuildingAssessmentsHandler
{
    /// <summary>Returns a paginated list of <see cref="BuildingAssessmentView"/>, ordered newest first.</summary>
    public static async Task<PaginatedResponse<BuildingAssessmentView>> Handle(
        GetBuildingAssessments request,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var page = await session.Query<BuildingAssessmentView>()
            .Where(x => x.BuildingId == request.BuildingId.Value)
            .OrderByDescending(x => x.AssessmentDate)
            .ThenByDescending(x => x.RecordedAt)
            .ToPagedListAsync(request.Page.Number, request.Page.Size, cancellationToken)
            .ConfigureAwait(false);

        return new PaginatedResponse<BuildingAssessmentView>
        {
            Items = [.. page],
            TotalCount = (int)page.TotalItemCount,
            PageSize = (int)page.PageSize,
            PageNumber = (int)page.PageNumber,
            TotalPages = (int)page.PageCount,
        };
    }
}
