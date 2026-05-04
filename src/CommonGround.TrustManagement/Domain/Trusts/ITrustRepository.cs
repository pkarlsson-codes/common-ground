using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Persistence;

namespace CommonGround.TrustManagement.Domain.Trusts;

/// <summary>Repository interface for managing trust entities.</summary>
public interface ITrustRepository : IRepository<Trust, TrustId>
{
}