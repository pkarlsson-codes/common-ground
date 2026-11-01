using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.PropertyRegistry.Application.Commands;

/// <summary>Registers a new land parcel in the CLT portfolio.</summary>
/// <param name="LandParcelId">The new parcel's ID, chosen by the caller.</param>
/// <param name="LegalParcelNumber">The legal parcel identifier (APN or description).</param>
/// <param name="Address">The street address of the parcel.</param>
/// <param name="AreaAcres">The area of the parcel in acres.</param>
/// <param name="AcquisitionDate">The date the CLT acquired this parcel.</param>
/// <param name="AcquisitionMethod">How the CLT acquired this parcel.</param>
/// <param name="CovenantTerms">Optional free-text affordability covenant description.</param>
/// <param name="RegisteredBy">The staff user registering the parcel.</param>
public sealed record RegisterLandParcel(
    LandParcelId LandParcelId,
    string LegalParcelNumber,
    string Address,
    decimal AreaAcres,
    DateOnly AcquisitionDate,
    AcquisitionMethod AcquisitionMethod,
    string? CovenantTerms,
    UserId RegisteredBy);

/// <summary>Handles <see cref="RegisterLandParcel"/> by creating and persisting a new LandParcel.</summary>
public static class RegisterLandParcelHandler
{
    /// <summary>Starts the new parcel's stream.</summary>
    public static IStartStream Handle(RegisterLandParcel command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var registered = LandParcel.Register(
            command.LandParcelId,
            command.LegalParcelNumber,
            command.Address,
            command.AreaAcres,
            command.AcquisitionDate,
            command.AcquisitionMethod,
            command.CovenantTerms,
            command.RegisteredBy);

        return MartenOps.StartStream<LandParcel>(command.LandParcelId.Value, registered);
    }
}
