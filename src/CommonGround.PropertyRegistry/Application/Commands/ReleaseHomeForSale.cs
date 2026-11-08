using CommonGround.PropertyRegistry.Application.EventHandlers;
using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Domain.Homes;
using CommonGround.PropertyRegistry.Application.Errors;
using CommonGround.SharedKernel.Domain.Services;
using Marten;
using Wolverine.Marten;

namespace CommonGround.PropertyRegistry.Application.Commands;

/// <summary>Offers a home the trust bought back and chose to hold to the waiting list.</summary>
/// <param name="HomeId">The held home.</param>
public sealed record ReleaseHomeForSale(HomeId HomeId);

/// <summary>What releasing a home decides from.</summary>
/// <param name="Availability">The home's availability, if any news of its sale has arrived.</param>
/// <param name="Today">The trust's today.</param>
public sealed record HomeRelease(HomeAvailabilityView? Availability, DateOnly Today);

/// <summary>Handles <see cref="ReleaseHomeForSale"/>.</summary>
public static class ReleaseHomeForSaleHandler
{
    /// <summary>Loads the home's availability and the trust's today.</summary>
    public static async Task<HomeRelease> LoadAsync(
        ReleaseHomeForSale command,
        IDocumentSession session,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(trustClock);

        var availability = await HomeAvailabilityUpdate.LoadAsync(session, command.HomeId.Value, cancellationToken).ConfigureAwait(false);
        var today = await trustClock.TodayAsync(session.TenantId, cancellationToken).ConfigureAwait(false);

        return new HomeRelease(availability, today);
    }

    /// <summary>Ends the hold on the home's availability.</summary>
    public static IMartenOp Handle(ReleaseHomeForSale command, HomeRelease release)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(release);

        if (release.Availability is null)
            throw BusinessRuleViolation.Home.ReleaseForSale.NotHeld();

        return HomeAvailabilityUpdate.Record(command.HomeId.Value, release.Availability, view => view.Release(release.Today));
    }
}
