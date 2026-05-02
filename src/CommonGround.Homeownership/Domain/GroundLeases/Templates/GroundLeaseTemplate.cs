using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.Homeownership.Domain.GroundLeases.Templates.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;
using CommonGround.SharedKernel.Domain.Exceptions;

namespace CommonGround.Homeownership.Domain.GroundLeases.Templates;

/// <summary>
/// A reusable template for creating ground leases.
/// </summary>
public sealed class GroundLeaseTemplate : AggregateRoot<GroundLeaseTemplateId>
{
#pragma warning disable CS8618
    private GroundLeaseTemplate() { }
#pragma warning restore CS8618

    /// <summary>Gets the template display name.</summary>
    public string TemplateName { get; private set; }

    /// <summary>Gets the lease term in years.</summary>
    public int TermYears { get; private set; }

    /// <summary>Gets the covenant terms.</summary>
    public string CovenantTerms { get; private set; }

    /// <summary>Gets the resale policy terms.</summary>
    public ResalePolicyTerms ResalePolicyTerms { get; private set; }

    /// <summary>Gets whether this template is active.</summary>
    public GroundLeaseTemplateStatus Status { get; private set; }

    /// <summary>Gets when this template was created.</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Gets the user who created this template.</summary>
    public UserId CreatedBy { get; private set; }

    /// <summary>Creates a new ground lease template.</summary>
    public static GroundLeaseTemplate Create(
        string templateName,
        int termYears,
        string covenantTerms,
        ResalePolicyTerms resalePolicyTerms,
        UserId createdBy)
    {
        ArgumentNullException.ThrowIfNull(resalePolicyTerms);
        ArgumentNullException.ThrowIfNull(createdBy);

        if (string.IsNullOrWhiteSpace(templateName))
        {
            throw new BusinessRuleViolationException("Template name is required.");
        }

        if (termYears <= 0)
        {
            throw new BusinessRuleViolationException("Term years must be positive.");
        }

        if (string.IsNullOrWhiteSpace(covenantTerms))
        {
            throw new BusinessRuleViolationException("Covenant terms must be specified.");
        }

        var template = new GroundLeaseTemplate();
        template.ApplyChange(new GroundLeaseTemplateCreated(
            GroundLeaseTemplateId.Generate().Value,
            templateName,
            termYears,
            covenantTerms,
            resalePolicyTerms.FormulaKind,
            resalePolicyTerms.AppreciationShare,
            resalePolicyTerms.IndexType,
            resalePolicyTerms.BaselineIndexValue,
            resalePolicyTerms.AnnualPercentage,
            resalePolicyTerms.ImprovementsCap,
            createdBy.Value,
            DateTimeOffset.UtcNow));

        return template;
    }

    /// <summary>Activates this template.</summary>
    public void Activate(UserId activatedBy)
    {
        ArgumentNullException.ThrowIfNull(activatedBy);

        if (Status == GroundLeaseTemplateStatus.Active)
        {
            throw new BusinessRuleViolationException(
                "Ground lease template is already active.");
        }

        ApplyChange(new GroundLeaseTemplateRestored(
            Id.Value,
            activatedBy.Value,
            DateTimeOffset.UtcNow));
    }

    /// <summary>Deactivates this template.</summary>
    public void Deactivate(UserId deactivatedBy)
    {
        ArgumentNullException.ThrowIfNull(deactivatedBy);

        if (Status == GroundLeaseTemplateStatus.Inactive)
        {
            throw new BusinessRuleViolationException(
                "Ground lease template is already inactive.");
        }

        ApplyChange(new GroundLeaseTemplateArchived(
            Id.Value,
            deactivatedBy.Value,
            DateTimeOffset.UtcNow));
    }

    /// <inheritdoc/>
    protected override void ApplyEvent(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case GroundLeaseTemplateCreated e: Apply(e); break;
            case GroundLeaseTemplateArchived e: Apply(e); break;
            case GroundLeaseTemplateRestored e: Apply(e); break;
            default: throw new InvalidOperationException(
                $"Unknown event: {domainEvent?.GetType().Name}");
        }
    }

    private void Apply(GroundLeaseTemplateCreated e)
    {
        Id = GroundLeaseTemplateId.Create(e.AggregateId);
        TemplateName = e.TemplateName;
        TermYears = e.TermYears;
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

    private void Apply(GroundLeaseTemplateArchived _)
    {
        Status = GroundLeaseTemplateStatus.Inactive;
    }

    private void Apply(GroundLeaseTemplateRestored _)
    {
        Status = GroundLeaseTemplateStatus.Active;
    }
}
