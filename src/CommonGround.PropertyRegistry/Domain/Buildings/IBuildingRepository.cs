using CommonGround.SharedKernel.Persistence;

namespace CommonGround.PropertyRegistry.Domain.Buildings;

/// <summary>Repository for <see cref="Building"/> aggregates.</summary>
public interface IBuildingRepository : IRepository<Building, BuildingId>
{
}