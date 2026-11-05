using CommonGround.Homeownership.Application.ReadModels;
using Marten;
using Wolverine.Marten;

namespace CommonGround.Homeownership.Application.EventHandlers;

/// <summary>Loads and writes a <see cref="HomeSelectionView"/> for the handlers that keep it.</summary>
public static class HomeSelectionUpdate
{
    /// <summary>Loads the selection, if any news of it has arrived yet.</summary>
    public static Task<HomeSelectionView?> LoadAsync(IQuerySession session, string homeSelectionId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentException.ThrowIfNullOrWhiteSpace(homeSelectionId);

        return session.LoadAsync<HomeSelectionView>(homeSelectionId, cancellationToken);
    }

    /// <summary>
    /// Records news of the selection, starting the view when this is the first. The view is stored with
    /// optimistic concurrency, so news handled at the same time is retried rather than overwritten.
    /// </summary>
    public static IMartenOp Record(string homeSelectionId, string homeId, HomeSelectionView? selection, Action<HomeSelectionView> record)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeSelectionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);
        ArgumentNullException.ThrowIfNull(record);

        var view = selection ?? new HomeSelectionView { Id = homeSelectionId, HomeId = homeId };
        record(view);

        return selection is null ? MartenOps.Insert(view) : MartenOps.Update(view);
    }
}
