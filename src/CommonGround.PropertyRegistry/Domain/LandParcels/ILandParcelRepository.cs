using CommonGround.SharedKernel.Persistence;

namespace CommonGround.PropertyRegistry.Domain.LandParcels;

/// <summary>Repository for <see cref="LandParcel"/> aggregates.</summary>
public interface ILandParcelRepository : IRepository<LandParcel, LandParcelId>
{
}
