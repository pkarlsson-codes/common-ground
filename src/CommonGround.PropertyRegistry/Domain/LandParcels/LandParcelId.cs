using CommonGround.SharedKernel.Domain;

namespace CommonGround.PropertyRegistry.Domain.LandParcels;

/// <summary>Strongly-typed identifier for a LandParcel.</summary>
/// <remarks>Initializes a new instance of <see cref="LandParcelId"/>.</remarks>
public sealed class LandParcelId(string value)
: EntityId<LandParcelId>(value, Prefix)
{
    private const string Prefix = "parcel";
}    
