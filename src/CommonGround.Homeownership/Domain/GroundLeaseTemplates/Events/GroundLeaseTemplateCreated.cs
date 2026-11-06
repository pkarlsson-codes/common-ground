using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeaseTemplates.Events;

/// <summary>Raised when a new ground lease template is created.</summary>
public sealed record GroundLeaseTemplateCreated(
    string AggregateId,
    string TemplateName,
    int TermYears,
    int OptionWindowDays,
    string CovenantTerms,
    FormulaKind FormulaKind,
    decimal? AppreciationShare,
    string? IndexType,
    decimal? BaselineIndexValue,
    decimal? AnnualPercentage,
    decimal? ImprovementsCap,
    string CreatedBy) : DomainEvent(AggregateId);
