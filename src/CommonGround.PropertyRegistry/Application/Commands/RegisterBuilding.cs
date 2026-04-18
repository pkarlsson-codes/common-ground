using CommonGround.PropertyRegistry.Domain.Buildings;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Persistence;

namespace CommonGround.PropertyRegistry.Application.Commands;

/// <summary>Registers a new structure on a CLT land parcel.</summary>
/// <param name="LandParcelId">The parcel this structure sits on.</param>
/// <param name="Type">The physical type of the structure.</param>
/// <param name="Bedrooms">Number of bedrooms.</param>
/// <param name="Bathrooms">Number of bathrooms (supports 1.5 etc.).</param>
/// <param name="SquareFootage">Total square footage.</param>
/// <param name="YearBuilt">Year the structure was built.</param>
/// <param name="InitialAssessedValue">Initial assessed value in dollars.</param>
/// <param name="RegisteredBy">The staff user registering the structure.</param>
public sealed record RegisterBuilding(
    LandParcelId LandParcelId,
    BuildingType Type,
    int Bedrooms,
    decimal Bathrooms,
    decimal SquareFootage,
    int YearBuilt,
    decimal InitialAssessedValue,
    UserId RegisteredBy);

/// <summary>Handles <see cref="RegisterBuilding"/> by creating and persisting a new Structure.</summary>
public static class RegisterStructureHandler
{
    /// <summary>Creates a new Structure and returns its ID.</summary>
    public static async Task<BuildingId> Handle(
        RegisterBuilding command,
        IBuildingRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var structure = Building.Register(
            command.LandParcelId,
            command.Type,
            command.Bedrooms,
            command.Bathrooms,
            command.SquareFootage,
            command.YearBuilt,
            command.InitialAssessedValue,
            command.RegisteredBy);

        repository.Stage(structure);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

        return structure.Id;
    }
}