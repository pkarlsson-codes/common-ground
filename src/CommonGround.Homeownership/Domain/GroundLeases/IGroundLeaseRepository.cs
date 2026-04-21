using CommonGround.SharedKernel.Persistence;

namespace CommonGround.Homeownership.Domain.GroundLeases;

/// <summary>Repository interface for ground lease aggregates.</summary>
public interface IGroundLeaseRepository : IRepository<GroundLease, GroundLeaseId>;
