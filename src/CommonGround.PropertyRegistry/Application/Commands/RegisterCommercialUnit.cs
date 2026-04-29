using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.PropertyRegistry.Domain.LeasableUnits;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Persistence;

namespace CommonGround.PropertyRegistry.Application.Commands;

/// <summary>Registers a new commercial unit on a CLT land parcel.</summary>
/// <param name="LandParcelId">The parcel this unit sits on.</param>
/// <param name="SquareFootage">Total square footage.</param>
/// <param name="YearBuilt">Year the unit was built.</param>
/// <param name="InitialAssessedValue">Initial assessed value in dollars.</param>
/// <param name="RegisteredBy">The staff user registering the unit.</param>
public sealed record RegisterCommercialUnit(
    LandParcelId LandParcelId,
    decimal SquareFootage,
    int YearBuilt,
    decimal InitialAssessedValue,
    UserId RegisteredBy);

/// <summary>Handles <see cref="RegisterCommercialUnit"/> by creating and persisting a new CommercialUnit.</summary>
public static class RegisterCommercialUnitHandler
{
    /// <summary>Creates a new CommercialUnit and returns its ID.</summary>
    public static async Task<LeasableUnitId> Handle(
        RegisterCommercialUnit command,
        ILeasableUnitRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var unit = CommercialUnit.Register(
            command.LandParcelId,
            command.SquareFootage,
            command.YearBuilt,
            command.InitialAssessedValue,
            command.RegisteredBy);

        repository.Stage(unit);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

        return unit.Id;
    }
}
