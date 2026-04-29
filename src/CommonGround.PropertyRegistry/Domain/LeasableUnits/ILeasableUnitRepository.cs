using CommonGround.SharedKernel.Persistence;

namespace CommonGround.PropertyRegistry.Domain.LeasableUnits;

/// <summary>Repository for <see cref="LeasableUnit"/> aggregates.</summary>
public interface ILeasableUnitRepository : IRepository<LeasableUnit, LeasableUnitId>
{
}
