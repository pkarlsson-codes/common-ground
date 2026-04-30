using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Domain.Homes;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

namespace CommonGround.PropertyRegistry.Application.Queries;

/// <summary>Returns a single home by ID.</summary>
/// <param name="UnitId">The ID of the home to fetch.</param>
public sealed record GetHome(HomeId UnitId);

/// <summary>Handles <see cref="GetHome"/>.</summary>
public static class GetHomeHandler
{
    /// <summary>Returns the <see cref="HomeView"/> or throws if not found.</summary>
    public static async Task<HomeView> Handle(
        GetHome request,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var view = await session.LoadAsync<HomeView>(request.UnitId.Value, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(request.UnitId.Value);

        return view;
    }
}
