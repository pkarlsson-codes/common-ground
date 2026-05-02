using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeases.Events;

/// <summary>Raised when a new ground lease is created from a ground lease template.</summary>
public sealed record GroundLeaseCreatedFromTemplate(
    string AggregateId,
    string GroundLeaseTemplateId,
    string LandParcelId,
    string BuildingId,
    DateOnly LeaseStartDate,
    int TermYears,
    string CovenantTerms,
    FormulaKind FormulaKind,
    decimal? AppreciationShare,
    string? IndexType,
    decimal? BaselineIndexValue,
    decimal? AnnualPercentage,
    decimal? ImprovementsCap,
    string RegisteredBy,
    DateTimeOffset OccurredAt) : IDomainEvent;
