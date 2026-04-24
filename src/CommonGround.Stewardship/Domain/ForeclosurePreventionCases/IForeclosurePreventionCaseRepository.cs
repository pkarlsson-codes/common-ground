using CommonGround.SharedKernel.Persistence;

namespace CommonGround.Stewardship.Domain.ForeclosurePreventionCases;

/// <summary>Repository for foreclosure prevention case aggregates.</summary>
public interface IForeclosurePreventionCaseRepository
    : IRepository<ForeclosurePreventionCase, ForeclosurePreventionCaseId>;
