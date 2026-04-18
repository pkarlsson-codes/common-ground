using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Domain.Buildings;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

namespace CommonGround.PropertyRegistry.Application.Queries;

/// <summary>Returns a single building by ID.</summary>
/// <param name="BuildingId">The ID of the building to fetch.</param>
public sealed record GetBuilding(BuildingId BuildingId);

/// <summary>Handles <see cref="GetBuilding"/>.</summary>
public static class GetBuildingHandler
{
    /// <summary>Returns the <see cref="BuildingView"/> or throws if not found.</summary>
    public static async Task<BuildingView> Handle(
        GetBuilding request,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var view = await session.LoadAsync<BuildingView>(request.BuildingId.Value, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(request.BuildingId.Value);

        return view;
    }
}