using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeaseTemplates.Events;
using Marten.Events.Aggregation;

namespace CommonGround.Homeownership.Infrastructure.Persistence.Projections;

/// <summary>Builds <see cref="GroundLeaseTemplateDetailView"/> from GroundLeaseTemplate events.</summary>
internal sealed class GroundLeaseTemplateDetailViewProjection : SingleStreamProjection<GroundLeaseTemplateDetailView, string>
{
    /// <summary>
    /// Creates a new <see cref="GroundLeaseTemplateDetailView"/> when a <see cref="GroundLeaseTemplateCreated"/> event is applied.
    /// </summary>
    /// <param name="e">Ground lease template created event.</param>
    /// <returns>A new instance of <see cref="GroundLeaseTemplateDetailView"/> initialized with the event data.</returns>
    public static GroundLeaseTemplateDetailView Create(GroundLeaseTemplateCreated e) =>
        new()
        {
            Id = e.AggregateId,
            TemplateName = e.TemplateName,
            TermYears = e.TermYears,
            OptionWindowDays = e.OptionWindowDays,
            LandUseFee = e.LandUseFee,
            RepairReserveFee = e.RepairReserveFee,
            LateInterestRate = e.LateInterestRate,
            InterestWaiverDays = e.InterestWaiverDays,
            CovenantTerms = e.CovenantTerms,
            Status = "Active",
            FormulaKind = e.FormulaKind.ToString(),
            AppreciationShare = e.AppreciationShare,
            IndexType = e.IndexType,
            BaselineIndexValue = e.BaselineIndexValue,
            AnnualPercentage = e.AnnualPercentage,
            ImprovementsCap = e.ImprovementsCap,
            CreatedAt = e.OccurredAt,
            CreatedBy = e.CreatedBy,
        };

    /// <summary>
    /// Marks the template as inactive in the detail view when it is archived.
    /// </summary>
    /// <param name="view">The detail view to update.</param>
    /// <param name="e">Ground lease template archived event.</param>
    public static void Apply(GroundLeaseTemplateDetailView view, GroundLeaseTemplateArchived e)
    {
        view.Status = "Inactive";
    }

    /// <summary>
    /// Reactivates the template in the detail view when a template is restored.
    /// </summary>
    /// <param name="view">The detail view to update.</param>
    /// <param name="e">Ground lease template restored event.</param>
    public static void Apply(GroundLeaseTemplateDetailView view, GroundLeaseTemplateRestored e)
    {
        view.Status = "Active";
    }
}
