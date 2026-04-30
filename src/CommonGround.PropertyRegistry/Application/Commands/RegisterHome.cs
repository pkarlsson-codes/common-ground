using CommonGround.PropertyRegistry.Domain.Homes;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Persistence;

namespace CommonGround.PropertyRegistry.Application.Commands;

/// <summary>Registers a new home on a CLT land parcel.</summary>
/// <param name="LandParcelId">The parcel this home sits on.</param>
/// <param name="UnitType">The physical type of the home.</param>
/// <param name="Bedrooms">Number of bedrooms.</param>
/// <param name="Bathrooms">Number of bathrooms (supports 1.5 etc.).</param>
/// <param name="SquareFootage">Total square footage.</param>
/// <param name="YearBuilt">Year the home was built.</param>
/// <param name="InitialAssessedValue">Initial assessed value in dollars.</param>
/// <param name="RegisteredBy">The staff user registering the home.</param>
public sealed record RegisterHome(
    LandParcelId LandParcelId,
    HomeType UnitType,
    int Bedrooms,
    decimal Bathrooms,
    decimal SquareFootage,
    int YearBuilt,
    decimal InitialAssessedValue,
    UserId RegisteredBy);

/// <summary>Handles <see cref="RegisterHome"/> by creating and persisting a new Home.</summary>
public static class RegisterHomeHandler
{
    /// <summary>Creates a new Home and returns its ID.</summary>
    public static async Task<HomeId> Handle(
        RegisterHome command,
        IHomeRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var unit = Home.Register(
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
