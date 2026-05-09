using CommonGround.Homeownership.Domain.GroundLeases.Events;
using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.Homeownership.Domain.GroundLeaseTemplates;
using CommonGround.Homeownership.Application.Errors;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;
using CommonGround.Homeownership.Domain.PurchaseWorkflows;

namespace CommonGround.Homeownership.Domain.GroundLeases;

/// <summary>
/// A ground lease between the CLT and a homeowner.
/// The CLT retains land ownership; the homeowner leases the land and owns the structure.
/// </summary>
public sealed class GroundLease : AggregateRoot<GroundLeaseId>
{
#pragma warning disable CS8618
    private GroundLease() { }
#pragma warning restore CS8618

    /// <summary>Gets the ID of the land parcel this lease is for.</summary>
    public string LandParcelId { get; private set; }

    /// <summary>Gets the ID of the leasable unit on this leased parcel.</summary>
    public string UnitId { get; private set; }

    /// <summary>Gets the start date of the lease.</summary>
    public DateOnly LeaseStartDate { get; private set; }

    /// <summary>Gets the term of the lease in years.</summary>
    public int TermYears { get; private set; }

    /// <summary>Gets the covenant terms for this lease.</summary>
    public string CovenantTerms { get; private set; }

    /// <summary>Gets the resale policy terms for this lease.</summary>
    public ResalePolicyTerms ResalePolicyTerms { get; private set; }

    /// <summary>Gets the current homeowner, if any.</summary>
    public Homeowner? CurrentHomeowner { get; private set; }

    /// <summary>Gets the status of this ground lease.</summary>
    public GroundLeaseStatus Status { get; private set; }

    /// <summary>Gets when this lease was created.</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Gets the user who created this lease.</summary>
    public UserId CreatedBy { get; private set; }

    /// <summary>
    /// Gets the source template ID when this lease was created from a template.
    /// Null indicates this lease was created directly without a template.
    /// </summary>
    public GroundLeaseTemplateId? TemplateId { get; private set; }

    /// <summary>Gets the source purchase workflow ID when this lease was created from a purchase workflow.</summary>
    public PurchaseWorkflowId? PurchaseWorkflowId { get; private set; } 

    /// <summary>Creates a new ground lease.</summary>
    public static GroundLease Create(
        string landParcelId,
        string unitId,
        DateOnly leaseStartDate,
        int termYears,
        string covenantTerms,
        ResalePolicyTerms resalePolicyTerms,
        UserId createdBy)
    {
        ArgumentNullException.ThrowIfNull(landParcelId);
        ArgumentNullException.ThrowIfNull(unitId);
        ArgumentNullException.ThrowIfNull(resalePolicyTerms);
        ArgumentNullException.ThrowIfNull(createdBy);

        if (termYears <= 0)
            throw BusinessRuleViolation.GroundLease.Create.TermYearsNonPositive(termYears);

        if (string.IsNullOrWhiteSpace(covenantTerms))
            throw BusinessRuleViolation.GroundLease.Create.CovenantTermsRequired();

        var lease = new GroundLease();
        lease.ApplyChange(new GroundLeaseCreated(
            GroundLeaseId.Generate().Value,
            landParcelId,
            unitId,
            leaseStartDate,
            termYears,
            covenantTerms,
            resalePolicyTerms.FormulaKind,
            resalePolicyTerms.AppreciationShare,
            resalePolicyTerms.IndexType,
            resalePolicyTerms.BaselineIndexValue,
            resalePolicyTerms.AnnualPercentage,
            resalePolicyTerms.ImprovementsCap,
            createdBy.Value));

        return lease;
    }

    /// <summary>Creates a new ground lease from an active template.</summary>
    public static GroundLease CreateFromPurchase(
        GroundLeaseTemplate template,
        PurchaseWorkflow purchaseWorkflow,
        DateOnly leaseStartDate,
        UserId createdBy)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(purchaseWorkflow);
        ArgumentNullException.ThrowIfNull(createdBy);

        if (template.Status != GroundLeaseTemplateStatus.Active)
        {
            throw BusinessRuleViolation.GroundLease.CreateFromTemplate.TemplateNotActive(template.Status);
        }

        var lease = new GroundLease();
        lease.ApplyChange(new GroundLeaseCreatedFromPurchase(
            GroundLeaseId.Generate().Value,
            purchaseWorkflow.Id.Value,
            template.Id.Value,
            purchaseWorkflow.LandParcelId,
            purchaseWorkflow.HomeId,
            leaseStartDate,
            template.TermYears,
            template.CovenantTerms,
            template.ResalePolicyTerms.FormulaKind,
            template.ResalePolicyTerms.AppreciationShare,
            template.ResalePolicyTerms.IndexType,
            template.ResalePolicyTerms.BaselineIndexValue,
            template.ResalePolicyTerms.AnnualPercentage,
            template.ResalePolicyTerms.ImprovementsCap,
            createdBy.Value,
            purchaseWorkflow.Buyer.FullName,
            purchaseWorkflow.Buyer.Email,
            purchaseWorkflow.Buyer.Phone));

        return lease;
    }

    /// <summary>Terminates this ground lease.</summary>
    /// <param name="reason">The reason for termination.</param>
    /// <param name="terminatedDate">The date of termination.</param>
    /// <param name="terminatedBy">The user recording this action.</param>
    public void Terminate(string reason, DateOnly terminatedDate, UserId terminatedBy)
    {
        ArgumentNullException.ThrowIfNull(terminatedBy);

        if (string.IsNullOrWhiteSpace(reason))
            throw BusinessRuleViolation.GroundLease.Terminate.ReasonRequired();

        if (Status != GroundLeaseStatus.Active)
            throw BusinessRuleViolation.GroundLease.Terminate.StatusNotActive(Status);

        ApplyChange(new GroundLeaseTerminated(
            Id.Value,
            reason,
            terminatedDate,
            terminatedBy.Value));
    }

    /// <summary>Records a stewardship check-in visit.</summary>
    /// <param name="checkInDate">Date of the check-in.</param>
    /// <param name="notes">Notes from the visit.</param>
    /// <param name="recordedBy">The user recording this action.</param>
    public void RecordStewardshipCheckIn(DateOnly checkInDate, string notes, UserId recordedBy)
    {
        ArgumentNullException.ThrowIfNull(recordedBy);

        if (string.IsNullOrWhiteSpace(notes))
        {
            throw BusinessRuleViolation.GroundLease.RecordStewardshipCheckIn.NotesRequired();
        }

        ApplyChange(new StewardshipCheckInRecorded(
            Id.Value,
            checkInDate,
            notes,
            recordedBy.Value));
    }

    /// <inheritdoc/>
    protected override void ApplyEvent(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case GroundLeaseCreated e: Apply(e); break;
            case GroundLeaseCreatedFromPurchase e: Apply(e); break;
            case StewardshipCheckInRecorded: break; // No state mutation needed
            case GroundLeaseTerminated e: Apply(e); break;
            default: throw new InvalidOperationException(
                $"Unknown event: {domainEvent?.GetType().Name}");
        }
    }

    private void Apply(GroundLeaseCreated e)
    {
        Id = GroundLeaseId.Create(e.AggregateId);
        LandParcelId = e.LandParcelId;
        UnitId = e.HomeId;
        LeaseStartDate = e.LeaseStartDate;
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
        Status = GroundLeaseStatus.Active;
        CreatedBy = UserId.Create(e.RegisteredBy);
        CreatedAt = e.OccurredAt;
        TemplateId = null;
    }

    private void Apply(GroundLeaseCreatedFromPurchase e)
    {
        Id = GroundLeaseId.Create(e.AggregateId);
        PurchaseWorkflowId = PurchaseWorkflowId.Create(e.PurchaseWorkflowId);
        TemplateId = GroundLeaseTemplateId.Create(e.GroundLeaseTemplateId);
        LandParcelId = e.LandParcelId;
        UnitId = e.HomeId;
        LeaseStartDate = e.LeaseStartDate;
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
        Status = GroundLeaseStatus.Active;
        CreatedBy = UserId.Create(e.RegisteredBy);
        CreatedAt = e.OccurredAt;
        CurrentHomeowner = new Homeowner(e.BuyerFullName, e.BuyerEmail, e.BuyerPhone);
    }

    private void Apply(GroundLeaseTerminated _)
    {
        Status = GroundLeaseStatus.Inactive;
    }

}
