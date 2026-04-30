using CommonGround.SharedKernel.Persistence;

namespace CommonGround.PropertyRegistry.Domain.Homes;

/// <summary>Repository for <see cref="Home"/> aggregates.</summary>
public interface IHomeRepository : IRepository<Home, HomeId>
{
}
