using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
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
    public static Task<LandParcelId> Handle(
        RegisterLandParcel command,
        IDocumentSession session,
        CancellationToken cancellationToken)
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

        parcel.ValidateInvariants();
        session.Events.StartStream<LandParcel>(parcel.Id.Value, parcel.UncommittedEvents.ToArray());
        parcel.UncommittedEvents.MarkAsCommitted();

        return Task.FromResult(parcel.Id);
    }
}
