using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Domain.DevelopmentProjects;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

namespace CommonGround.PropertyRegistry.Application.Queries;

/// <summary>Returns a single development project by ID.</summary>
/// <param name="ProjectId">The ID of the project to fetch.</param>
public sealed record GetDevelopmentProject(DevelopmentProjectId ProjectId);

/// <summary>Handles <see cref="GetDevelopmentProject"/>.</summary>
public static class GetDevelopmentProjectHandler
{
    /// <summary>Returns the <see cref="DevelopmentProjectListItemView"/> or throws if not found.</summary>
    public static async Task<DevelopmentProjectListItemView> Handle(
        GetDevelopmentProject request,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await session.LoadAsync<DevelopmentProjectListItemView>(request.ProjectId.Value, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(request.ProjectId.Value);
    }
}
