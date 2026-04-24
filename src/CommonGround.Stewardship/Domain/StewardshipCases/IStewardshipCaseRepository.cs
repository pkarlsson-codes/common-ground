using CommonGround.SharedKernel.Persistence;

namespace CommonGround.Stewardship.Domain.StewardshipCases;

/// <summary>Repository for stewardship case aggregates.</summary>
public interface IStewardshipCaseRepository
    : IRepository<StewardshipCase, StewardshipCaseId>
{
    /// <summary>Gets the active stewardship case for the given ground lease, or null if none exists.</summary>
    Task<StewardshipCase?> GetActiveByGroundLeaseIdAsync(string groundLeaseId, CancellationToken cancellationToken = default);
}
