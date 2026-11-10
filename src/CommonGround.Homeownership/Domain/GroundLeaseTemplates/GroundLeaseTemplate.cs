using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.Homeownership.Application.Errors;
using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.Homeownership.Domain.GroundLeaseTemplates.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeaseTemplates;

/// <summary>
/// A reusable template for creating ground leases.
/// </summary>
public sealed class GroundLeaseTemplate : AggregateRoot<GroundLeaseTemplateId>
{
#pragma warning disable CS8618 // State is set by Create from the stream's first event.
    private GroundLeaseTemplate() { }
#pragma warning restore CS8618

    /// <summary>Gets the template display name.</summary>
    public string TemplateName { get; private set; }

    /// <summary>Gets the lease term in years.</summary>
    public int TermYears { get; private set; }

    /// <summary>Gets the days the trust has to buy a home, or assign its option, after its homeowner gives notice to sell.</summary>
    public int OptionWindowDays { get; private set; }

    /// <summary>Gets the covenant terms.</summary>
    public string CovenantTerms { get; private set; }

    /// <summary>Gets the resale policy terms.</summary>
    public ResalePolicyTerms ResalePolicyTerms { get; private set; }

    /// <summary>Gets the monthly lease fee a lease from this template charges.</summary>
    public LeaseFeeTerms LeaseFeeTerms { get; private set; }

    /// <summary>Gets whether this template is active.</summary>
    public GroundLeaseTemplateStatus Status { get; private set; }

    /// <summary>Gets when this template was created.</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Gets the user who created this template.</summary>
    public UserId CreatedBy { get; private set; }

    /// <summary>Creates a new ground lease template.</summary>
    public static GroundLeaseTemplateCreated Define(
        GroundLeaseTemplateId id,
        string templateName,
        int termYears,
        int optionWindowDays,
        string covenantTerms,
        ResalePolicyTerms resalePolicyTerms,
        LeaseFeeTerms leaseFeeTerms,
        UserId createdBy)
    {
        ArgumentNullException.ThrowIfNull(resalePolicyTerms);
        ArgumentNullException.ThrowIfNull(leaseFeeTerms);

        if (string.IsNullOrWhiteSpace(templateName))
        {
            throw BusinessRuleViolation.GroundLeaseTemplate.Create.TemplateNameRequired();
        }

        if (termYears <= 0)
        {
            throw BusinessRuleViolation.GroundLeaseTemplate.Create.TermYearsNonPositive(termYears);
        }

        if (optionWindowDays <= 0)
        {
            throw BusinessRuleViolation.GroundLeaseTemplate.Create.OptionWindowDaysNonPositive(optionWindowDays);
        }

        if (string.IsNullOrWhiteSpace(covenantTerms))
        {
            throw BusinessRuleViolation.GroundLeaseTemplate.Create.CovenantTermsRequired();
        }

        return new GroundLeaseTemplateCreated(
            id.Value,
            templateName,
            termYears,
            optionWindowDays,
            leaseFeeTerms.LandUseFee,
            leaseFeeTerms.RepairReserveFee,
            covenantTerms,
            resalePolicyTerms.FormulaKind,
            resalePolicyTerms.AppreciationShare,
            resalePolicyTerms.IndexType,
            resalePolicyTerms.BaselineIndexValue,
            resalePolicyTerms.AnnualPercentage,
            resalePolicyTerms.ImprovementsCap,
            createdBy.Value);
    }

    /// <summary>Restores this template to active status.</summary>
    public GroundLeaseTemplateRestored Restore(UserId restoredBy)
    {
        if (Status == GroundLeaseTemplateStatus.Active)
        {
            throw BusinessRuleViolation.GroundLeaseTemplate.Restore.AlreadyActive();
        }

        return new GroundLeaseTemplateRestored(
            Id.Value,
            restoredBy.Value);
    }

    /// <summary>Archives this template, making it inactive.</summary>
    public GroundLeaseTemplateArchived Archive(UserId archivedBy)
    {
        if (Status == GroundLeaseTemplateStatus.Inactive)
        {
            throw BusinessRuleViolation.GroundLeaseTemplate.Archive.AlreadyInactive();
        }

        return new GroundLeaseTemplateArchived(
            Id.Value,
            archivedBy.Value);
    }

    /// <summary>Starts a ground lease template from its first event.</summary>
    public static GroundLeaseTemplate Create(GroundLeaseTemplateCreated e)
    {
        ArgumentNullException.ThrowIfNull(e);

        var groundLeaseTemplate = new GroundLeaseTemplate();
        groundLeaseTemplate.Apply(e);
        return groundLeaseTemplate;
    }

    private void Apply(GroundLeaseTemplateCreated e)
    {
        Id = GroundLeaseTemplateId.Create(e.AggregateId);
        TemplateName = e.TemplateName;
        TermYears = e.TermYears;
        OptionWindowDays = e.OptionWindowDays;
        LeaseFeeTerms = LeaseFeeTerms.Create(e.LandUseFee, e.RepairReserveFee);
        CovenantTerms = e.CovenantTerms;
        ResalePolicyTerms = e.FormulaKind switch
        {
            FormulaKind.AppraisalBased => ResalePolicyTerms.AppraisalBased(
                e.AppreciationShare!.Value, e.ImprovementsCap),
            FormulaKind.IndexBased => ResalePolicyTerms.IndexBased(
                e.IndexType!, e.BaselineIndexValue!.Value, e.ImprovementsCap),
            FormulaKind.FixedPercentage => ResalePolicyTerms.FixedPercentage(
                e.AnnualPercentage!.Value, e.ImprovementsCap),
            _ => throw new InvalidOperationException($"Unknown formula kind: {e.FormulaKind}")
        };
        Status = GroundLeaseTemplateStatus.Active;
        CreatedBy = UserId.Create(e.CreatedBy);
        CreatedAt = e.OccurredAt;
    }

    /// <summary>Applies <see cref="GroundLeaseTemplateArchived"/>.</summary>
    public void Apply(GroundLeaseTemplateArchived _)
    {
        Status = GroundLeaseTemplateStatus.Inactive;
    }

    /// <summary>Applies <see cref="GroundLeaseTemplateRestored"/>.</summary>
    public void Apply(GroundLeaseTemplateRestored _)
    {
        Status = GroundLeaseTemplateStatus.Active;
    }
}
