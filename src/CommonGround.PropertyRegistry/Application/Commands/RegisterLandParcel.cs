using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.PropertyRegistry.Application.Commands;

/// <summary>Registers a new land parcel in the CLT portfolio.</summary>
/// <param name="LegalParcelNumber">The legal parcel identifier (APN or description).</param>
/// <param name="Address">The street address of the parcel.</param>
/// <param name="AreaAcres">The area of the parcel in acres.</param>
/// <param name="AcquisitionDate">The date the CLT acquired this parcel.</param>
/// <param name="AcquisitionMethod">How the CLT acquired this parcel.</param>
/// <param name="CovenantTerms">Optional free-text affordability covenant description.</param>
/// <param name="RegisteredBy">The staff user registering the parcel.</param>
public sealed record RegisterLandParcel(
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
    /// <summary>Creates a new LandParcel and returns its ID.</summary>
    public static LandParcelId Handle(
        RegisterLandParcel command,
        IDocumentSession session)
    {
        ArgumentNullException.ThrowIfNull(command);

        var parcel = LandParcel.Register(
            command.LegalParcelNumber,
            command.Address,
            command.AreaAcres,
            command.AcquisitionDate,
            command.AcquisitionMethod,
            command.CovenantTerms,
            command.RegisteredBy);

        session.CreateAggregate<LandParcel, LandParcelId>(parcel);

        return parcel.Id;
    }
}
