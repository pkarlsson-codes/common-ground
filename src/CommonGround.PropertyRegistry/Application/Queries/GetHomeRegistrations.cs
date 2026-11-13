using CommonGround.PropertyRegistry.Application.ReadModels;
using Marten;

namespace CommonGround.PropertyRegistry.Application.Queries;

/// <summary>Returns when each of the trust's homes was registered.</summary>
public sealed record GetHomeRegistrations;

/// <summary>Every home and when it was registered.</summary>
/// <param name="Homes">The homes.</param>
public sealed record HomeRegistrations(IReadOnlyList<HomeRegistration> Homes);

/// <summary>A home and the moment it was registered.</summary>
/// <param name="HomeId">The home.</param>
/// <param name="RegisteredAt">When it was registered.</param>
public sealed record HomeRegistration(string HomeId, DateTimeOffset RegisteredAt);

/// <summary>Handles <see cref="GetHomeRegistrations"/>.</summary>
public static class GetHomeRegistrationsHandler
{
    /// <summary>Lists every home with its registration moment.</summary>
    public static async Task<HomeRegistrations> Handle(
        GetHomeRegistrations request,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);

        var homes = await session.Query<HomeView>()
            .Select(h => new { h.Id, h.RegisteredAt })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new HomeRegistrations([.. homes.Select(h => new HomeRegistration(h.Id, h.RegisteredAt))]);
    }
}
