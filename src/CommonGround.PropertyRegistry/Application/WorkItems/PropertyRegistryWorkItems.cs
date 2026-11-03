using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.SharedKernel.Application.WorkItems;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.PropertyRegistry.Application.WorkItems;

/// <summary>Derives the work items for homes waiting on a selection from PropertyRegistry's read models.</summary>
public static class PropertyRegistryWorkItems
{
    /// <summary>An available home that no selection is choosing a household for.</summary>
    public const string HomeAvailable = "home-available";

    /// <summary>Start choosing a household for the home.</summary>
    public const string StartSelection = "start-selection";

    /// <summary>
    /// The item for a home that is available with no selection open, waiting since it was registered or last
    /// became available again; none for a home in any other status.
    /// </summary>
    /// <param name="home">The home.</param>
    /// <param name="availability">Where the home stands in being sold; none means it is available.</param>
    /// <param name="label">The label staff know the home by.</param>
    /// <param name="zone">The trust's time zone, for the date the home was registered.</param>
    /// <returns>The home's work item, or null when it isn't waiting on a selection.</returns>
    public static WorkItem? ForHome(HomeView home, HomeAvailabilityView? availability, string label, IanaTimeZone zone)
    {
        ArgumentNullException.ThrowIfNull(home);
        ArgumentNullException.ThrowIfNull(zone);

        if (availability is not null && availability.Status != HomeStatus.Available.DisplayName)
            return null;

        return availability?.AvailableAgainDate is { } availableAgain
            ? new WorkItem(WorkArea.Selections, HomeAvailable, "home", home.Id, label,
                "Available again after a purchase fell through", null, availableAgain, [StartSelection])
            : new WorkItem(WorkArea.Selections, HomeAvailable, "home", home.Id, label,
                "Available, no selection started", null, zone.ToDate(home.RegisteredAt), [StartSelection]);
    }
}
