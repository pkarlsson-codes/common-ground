using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.PropertyRegistry.Domain.LeasableUnits;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Persistence;

namespace CommonGround.PropertyRegistry.Application.Commands;

/// <summary>Registers a new dwelling unit on a CLT land parcel.</summary>
/// <param name="LandParcelId">The parcel this unit sits on.</param>
/// <param name="UnitType">The physical type of the dwelling unit.</param>
/// <param name="Bedrooms">Number of bedrooms.</param>
/// <param name="Bathrooms">Number of bathrooms (supports 1.5 etc.).</param>
/// <param name="SquareFootage">Total square footage.</param>
/// <param name="YearBuilt">Year the unit was built.</param>
/// <param name="InitialAssessedValue">Initial assessed value in dollars.</param>
/// <param name="RegisteredBy">The staff user registering the unit.</param>
public sealed record RegisterDwellingUnit(
    LandParcelId LandParcelId,
    DwellingUnitType UnitType,
    int Bedrooms,
    decimal Bathrooms,
    decimal SquareFootage,
    int YearBuilt,
    decimal InitialAssessedValue,
    UserId RegisteredBy);

/// <summary>Handles <see cref="RegisterDwellingUnit"/> by creating and persisting a new DwellingUnit.</summary>
public static class RegisterDwellingUnitHandler
{
    /// <summary>Creates a new DwellingUnit and returns its ID.</summary>
    public static async Task<LeasableUnitId> Handle(
        RegisterDwellingUnit command,
        ILeasableUnitRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var unit = DwellingUnit.Register(
            command.LandParcelId,
            command.UnitType,
            command.Bedrooms,
            command.Bathrooms,
            command.SquareFootage,
            command.YearBuilt,
            command.InitialAssessedValue,
            command.RegisteredBy);

        repository.Stage(unit);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

        return unit.Id;
    }
}
