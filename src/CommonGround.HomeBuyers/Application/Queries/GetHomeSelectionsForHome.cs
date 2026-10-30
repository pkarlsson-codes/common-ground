using CommonGround.HomeBuyers.Application.ReadModels;
using Marten;

namespace CommonGround.HomeBuyers.Application.Queries;

/// <summary>Returns every selection started for a home, newest first.</summary>
public sealed record GetHomeSelectionsForHome(string HomeId);

/// <summary>Handles <see cref="GetHomeSelectionsForHome"/>.</summary>
public static class GetHomeSelectionsForHomeHandler
{
    /// <summary>
    /// Returns the home's selections, newest first. At most one of them is open or completed;
    /// the rest were cancelled.
    /// </summary>
    public static async Task<IReadOnlyList<HomeSelectionView>> Handle(
        GetHomeSelectionsForHome request,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await session.Query<HomeSelectionView>()
            .Where(v => v.HomeId == request.HomeId)
            .OrderByDescending(v => v.StartedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
