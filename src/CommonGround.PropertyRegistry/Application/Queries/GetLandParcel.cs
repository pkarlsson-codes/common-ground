using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

namespace CommonGround.PropertyRegistry.Application.Queries;

/// <summary>Returns a single land parcel by ID.</summary>
/// <param name="LandParcelId">The ID of the parcel to fetch.</param>
public sealed record GetLandParcel(LandParcelId LandParcelId);

/// <summary>Handles <see cref="GetLandParcel"/>.</summary>
public static class GetLandParcelHandler
{
    /// <summary>Returns the <see cref="LandParcelView"/> or throws if not found.</summary>
    public static async Task<LandParcelView> Handle(
        GetLandParcel request,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var view = await session.LoadAsync<LandParcelView>(request.LandParcelId.Value, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(request.LandParcelId.Value);

        return view;
    }
}