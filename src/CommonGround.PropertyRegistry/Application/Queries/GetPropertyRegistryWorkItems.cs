using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Application.WorkItems;
using CommonGround.PropertyRegistry.Domain.Homes;
using CommonGround.SharedKernel.Application.WorkItems;
using CommonGround.SharedKernel.Domain.Services;
using Marten;

namespace CommonGround.PropertyRegistry.Application.Queries;

/// <summary>Returns PropertyRegistry's work items: available homes that no selection is choosing for.</summary>
public sealed record GetPropertyRegistryWorkItems;

/// <summary>Handles <see cref="GetPropertyRegistryWorkItems"/>.</summary>
public static class GetPropertyRegistryWorkItemsHandler
{
    /// <summary>Loads the homes not known to be otherwise than available and derives their work items.</summary>
    public static async Task<WorkItemList> Handle(
        GetPropertyRegistryWorkItems request,
        IQuerySession session,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var zone = await trustClock.GetTimeZoneAsync(session.TenantId, cancellationToken).ConfigureAwait(false);

        // A home with no availability yet is available, so look past the ones known to be otherwise.
        var available = HomeStatus.Available.DisplayName;
        var otherwise = await session.Query<HomeAvailabilityView>()
            .Where(v => v.Status != available)
            .Select(v => v.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var homes = await session.Query<HomeView>()
            .Where(h => !otherwise.Contains(h.Id))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var availabilities = (await session
            .LoadManyAsync<HomeAvailabilityView>(cancellationToken, homes.Select(h => h.Id))
            .ConfigureAwait(false))
            .ToDictionary(a => a.Id);
        var addresses = (await session
            .LoadManyAsync<LandParcelView>(cancellationToken, homes.Select(h => h.LandParcelId).Distinct())
            .ConfigureAwait(false))
            .ToDictionary(p => p.Id, p => p.Address);

        return new WorkItemList([
            .. homes
                .Select(h => PropertyRegistryWorkItems.ForHome(
                    h,
                    availabilities.GetValueOrDefault(h.Id),
                    HomeLabel.Format(addresses.GetValueOrDefault(h.LandParcelId, string.Empty), h.UnitDesignation),
                    zone))
                .OfType<WorkItem>(),
        ]);
    }
}
