using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Domain.Homes;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

namespace CommonGround.PropertyRegistry.Application.Queries;

/// <summary>Returns a single home by ID.</summary>
/// <param name="UnitId">The ID of the home to fetch.</param>
public sealed record GetHome(HomeId UnitId);

/// <summary>A home, where it stands in being sold, and the label staff know it by.</summary>
public sealed record HomeDetails(HomeView Home, HomeAvailabilityView Availability, string Label);

/// <summary>Handles <see cref="GetHome"/>.</summary>
public static class GetHomeHandler
{
    /// <summary>Returns the home with its availability, labelled with its parcel's address, or throws if not found.</summary>
    public static async Task<HomeDetails> Handle(
        GetHome request,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var view = await session.LoadAsync<HomeView>(request.UnitId.Value, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(request.UnitId.Value);

        var availability = await session.LoadAsync<HomeAvailabilityView>(view.Id, cancellationToken).ConfigureAwait(false)
            ?? new HomeAvailabilityView { Id = view.Id };
        var parcel = await session.LoadAsync<LandParcelView>(view.LandParcelId, cancellationToken).ConfigureAwait(false);

        return new HomeDetails(view, availability, HomeLabel.Format(parcel?.Address ?? string.Empty, view.UnitDesignation));
    }
}
