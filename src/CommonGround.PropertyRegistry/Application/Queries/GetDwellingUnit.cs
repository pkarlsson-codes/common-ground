using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Domain.LeasableUnits;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

namespace CommonGround.PropertyRegistry.Application.Queries;

/// <summary>Returns a single dwelling unit by ID.</summary>
/// <param name="UnitId">The ID of the unit to fetch.</param>
public sealed record GetDwellingUnit(LeasableUnitId UnitId);

/// <summary>Handles <see cref="GetDwellingUnit"/>.</summary>
public static class GetDwellingUnitHandler
{
    /// <summary>Returns the <see cref="DwellingUnitView"/> or throws if not found.</summary>
    public static async Task<DwellingUnitView> Handle(
        GetDwellingUnit request,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var view = await session.LoadAsync<DwellingUnitView>(request.UnitId.Value, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(request.UnitId.Value);

        return view;
    }
}
