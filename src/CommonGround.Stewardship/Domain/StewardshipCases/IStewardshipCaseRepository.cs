using CommonGround.SharedKernel.Persistence;

namespace CommonGround.Stewardship.Domain.StewardshipCases;

/// <summary>Repository for stewardship case aggregates.</summary>
public interface IStewardshipCaseRepository
    : IRepository<StewardshipCase, StewardshipCaseId>;
