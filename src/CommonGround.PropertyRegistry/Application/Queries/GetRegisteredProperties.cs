using CommonGround.PropertyRegistry.Application.ReadModels;
using Marten;

namespace CommonGround.PropertyRegistry.Application.Queries;

/// <summary>Returns every land parcel and home in the trust, for matching imported rows to what already exists.</summary>
public sealed record GetRegisteredProperties;

/// <summary>A land parcel's ID and parcel number.</summary>
public sealed record RegisteredParcel(string Id, string LegalParcelNumber);

/// <summary>A home's ID, parcel and unit designation.</summary>
public sealed record RegisteredHome(string Id, string LandParcelId, string? UnitDesignation);

/// <summary>Every land parcel and home in the trust.</summary>
public sealed record RegisteredProperties(IReadOnlyList<RegisteredParcel> Parcels, IReadOnlyList<RegisteredHome> Homes);

/// <summary>Handles <see cref="GetRegisteredProperties"/>.</summary>
public static class GetRegisteredPropertiesHandler
{
    /// <summary>Loads every parcel, decommissioned or not, and every home.</summary>
    public static async Task<RegisteredProperties> Handle(
        GetRegisteredProperties request,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);

        var parcels = await session.Query<LandParcelListItemView>()
            .Select(p => new RegisteredParcel(p.Id, p.LegalParcelNumber))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var homes = await session.Query<HomeListItemView>()
            .Select(h => new RegisteredHome(h.Id, h.LandParcelId, h.UnitDesignation))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new RegisteredProperties([.. parcels], [.. homes]);
    }
}
