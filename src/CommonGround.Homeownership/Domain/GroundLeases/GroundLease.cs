using CommonGround.Homeownership.Domain.GroundLeases.Events;
using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.Homeownership.Domain.GroundLeaseTemplates;
using CommonGround.Homeownership.Application.Errors;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;
using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.Homeownership.Domain.Resales;

namespace CommonGround.Homeownership.Domain.GroundLeases;

/// <summary>
/// A ground lease between the CLT and a homeowner.
/// The CLT retains land ownership; the homeowner leases the land and owns the structure.
/// </summary>
public sealed class GroundLease : AggregateRoot<GroundLeaseId>
{
#pragma warning disable CS8618 // State is set by Create from the stream's first event.
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

    /// <summary>
    /// Gets the days the trust has to buy the home, or assign its option, after the homeowner gives notice to
    /// sell. Copied from the template when the lease was created, so editing the template later doesn't change it.
    /// </summary>
    public int OptionWindowDays { get; private set; }

    /// <summary>Gets what the homeowner paid for the home, which the resale formula starts from.</summary>
    public decimal BasePrice { get; private set; }

    /// <summary>Gets the home's appraised value when the homeowner bought it, if known.</summary>
    public decimal? OriginalAppraisedValue { get; private set; }

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

    /// <summary>Gets the resale in progress on this lease, if its homeowner has given notice to sell.</summary>
    public ResaleId? OpenResaleId { get; private set; }

    /// <summary>
    /// Creates a ground lease that didn't come from a purchase, such as one signed before the trust used
    /// CommonGround. Refused while the home is already leased, being sold, or in a home selection.
    /// </summary>
    public static GroundLeaseCreated Establish(
        GroundLeaseId id,
        string landParcelId,
        string unitId,
        Homeowner homeowner,
        DateOnly leaseStartDate,
        int termYears,
        int optionWindowDays,
        decimal basePrice,
        decimal originalAppraisedValue,
        string covenantTerms,
        ResalePolicyTerms resalePolicyTerms,
        HomeCommitments homeCommitments,
        UserId createdBy)
    {
        ArgumentNullException.ThrowIfNull(landParcelId);
        ArgumentNullException.ThrowIfNull(unitId);
        ArgumentNullException.ThrowIfNull(homeowner);
        ArgumentNullException.ThrowIfNull(resalePolicyTerms);
        ArgumentNullException.ThrowIfNull(homeCommitments);

        if (homeCommitments.ActiveLeaseId is { } activeLeaseId)
            throw BusinessRuleViolation.GroundLease.Create.HomeHasActiveLease(activeLeaseId);

        if (homeCommitments.OpenPurchaseWorkflowId is { } openPurchaseWorkflowId)
            throw BusinessRuleViolation.GroundLease.Create.HomeHasOpenPurchase(openPurchaseWorkflowId);

        if (homeCommitments.OpenHomeSelectionId is { } openHomeSelectionId)
            throw BusinessRuleViolation.GroundLease.Create.HomeInSelection(openHomeSelectionId);

        if (termYears <= 0)
            throw BusinessRuleViolation.GroundLease.Create.TermYearsNonPositive(termYears);

        if (optionWindowDays <= 0)
            throw BusinessRuleViolation.GroundLease.Create.OptionWindowDaysNonPositive(optionWindowDays);

        if (basePrice <= 0)
            throw BusinessRuleViolation.GroundLease.Create.BasePriceNonPositive(basePrice);

        if (originalAppraisedValue <= 0)
            throw BusinessRuleViolation.GroundLease.Create.OriginalAppraisedValueNonPositive(originalAppraisedValue);

        if (string.IsNullOrWhiteSpace(covenantTerms))
            throw BusinessRuleViolation.GroundLease.Create.CovenantTermsRequired();

        return new GroundLeaseCreated(
            id.Value,
            landParcelId,
            unitId,
            leaseStartDate,
            termYears,
            optionWindowDays,
            basePrice,
            originalAppraisedValue,
            covenantTerms,
            resalePolicyTerms.FormulaKind,
            resalePolicyTerms.AppreciationShare,
            resalePolicyTerms.IndexType,
            resalePolicyTerms.BaselineIndexValue,
            resalePolicyTerms.AnnualPercentage,
            resalePolicyTerms.ImprovementsCap,
            createdBy.Value,
            homeowner.FullName.Value,
            homeowner.Email.Value,
            homeowner.Phone?.Value);
    }

    /// <summary>Creates a new ground lease from an active template.</summary>
    public static GroundLeaseCreatedFromPurchase CreateFromPurchase(
        GroundLeaseId id,
        GroundLeaseTemplate template,
        PurchaseWorkflow purchaseWorkflow,
        DateOnly leaseStartDate,
        UserId createdBy)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(purchaseWorkflow);

        if (template.Status != GroundLeaseTemplateStatus.Active)
        {
            throw BusinessRuleViolation.GroundLease.CreateFromTemplate.TemplateNotActive(template.Status);
        }

        return new GroundLeaseCreatedFromPurchase(
            id.Value,
            purchaseWorkflow.Id.Value,
            template.Id.Value,
            purchaseWorkflow.LandParcelId,
            purchaseWorkflow.HomeId,
            leaseStartDate,
            template.TermYears,
            template.OptionWindowDays,
            purchaseWorkflow.PurchasePrice,
            purchaseWorkflow.AppraisalDetails?.AppraisedValue,
            template.CovenantTerms,
            template.ResalePolicyTerms.FormulaKind,
            template.ResalePolicyTerms.AppreciationShare,
            template.ResalePolicyTerms.IndexType,
            template.ResalePolicyTerms.BaselineIndexValue,
            template.ResalePolicyTerms.AnnualPercentage,
            template.ResalePolicyTerms.ImprovementsCap,
            createdBy.Value,
            purchaseWorkflow.Buyer.FullName.Value,
            purchaseWorkflow.Buyer.Email.Value,
            purchaseWorkflow.Buyer.Phone?.Value);
    }

    /// <summary>Terminates this ground lease.</summary>
    /// <param name="reason">The reason for termination.</param>
    /// <param name="terminatedDate">The date of termination.</param>
    /// <param name="terminatedBy">The user recording this action.</param>
    public GroundLeaseTerminated Terminate(string reason, DateOnly terminatedDate, UserId terminatedBy)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw BusinessRuleViolation.GroundLease.Terminate.ReasonRequired();

        if (Status != GroundLeaseStatus.Active)
            throw BusinessRuleViolation.GroundLease.Terminate.StatusNotActive(Status);

        return new GroundLeaseTerminated(
            Id.Value,
            reason,
            terminatedDate,
            terminatedBy.Value);
    }

    /// <summary>Records a stewardship check-in visit.</summary>
    /// <param name="checkInDate">Date of the check-in.</param>
    /// <param name="notes">Notes from the visit.</param>
    /// <param name="recordedBy">The user recording this action.</param>
    public StewardshipCheckInRecorded RecordStewardshipCheckIn(DateOnly checkInDate, string notes, UserId recordedBy)
    {
        if (string.IsNullOrWhiteSpace(notes))
        {
            throw BusinessRuleViolation.GroundLease.RecordStewardshipCheckIn.NotesRequired();
        }

        return new StewardshipCheckInRecorded(
            Id.Value,
            checkInDate,
            notes,
            recordedBy.Value);
    }

    /// <summary>
    /// Starts a resale on this lease when its homeowner gives notice to sell. Refused unless the lease is
    /// active, and while another resale is open on it.
    /// </summary>
    public ResaleStarted StartResale(ResaleId resaleId)
    {
        if (Status != GroundLeaseStatus.Active)
            throw BusinessRuleViolation.GroundLease.StartResale.StatusNotActive(Status);

        if (OpenResaleId is { } openResaleId)
            throw BusinessRuleViolation.GroundLease.StartResale.ResaleAlreadyOpen(openResaleId.Value);

        return new ResaleStarted(Id.Value, resaleId.Value);
    }

    /// <summary>Closes the lease's open resale, so its homeowner can give notice again. Nothing to do if it isn't open.</summary>
    public ResaleClosed? CloseResale(ResaleId resaleId) =>
        OpenResaleId == resaleId ? new ResaleClosed(Id.Value, resaleId.Value) : null;

    /// <summary>Starts a ground lease from its first event.</summary>
    public static GroundLease Create(GroundLeaseCreated e)
    {
        ArgumentNullException.ThrowIfNull(e);

        var groundLease = new GroundLease();
        groundLease.Apply(e);
        return groundLease;
    }

    private void Apply(GroundLeaseCreated e)
    {
        Id = GroundLeaseId.Create(e.AggregateId);
        LandParcelId = e.LandParcelId;
        UnitId = e.HomeId;
        LeaseStartDate = e.LeaseStartDate;
        TermYears = e.TermYears;
        OptionWindowDays = e.OptionWindowDays;
        BasePrice = e.BasePrice;
        OriginalAppraisedValue = e.OriginalAppraisedValue;
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
        CurrentHomeowner = Homeowner.From(e.HomeownerFullName, e.HomeownerEmail, e.HomeownerPhone);
    }

    /// <summary>Starts a ground lease from its first event.</summary>
    public static GroundLease Create(GroundLeaseCreatedFromPurchase e)
    {
        ArgumentNullException.ThrowIfNull(e);

        var groundLease = new GroundLease();
        groundLease.Apply(e);
        return groundLease;
    }

    private void Apply(GroundLeaseCreatedFromPurchase e)
    {
        Id = GroundLeaseId.Create(e.AggregateId);
        PurchaseWorkflowId = PurchaseWorkflows.PurchaseWorkflowId.Create(e.PurchaseWorkflowId);
        TemplateId = GroundLeaseTemplateId.Create(e.GroundLeaseTemplateId);
        LandParcelId = e.LandParcelId;
        UnitId = e.HomeId;
        LeaseStartDate = e.LeaseStartDate;
        TermYears = e.TermYears;
        OptionWindowDays = e.OptionWindowDays;
        BasePrice = e.BasePrice;
        OriginalAppraisedValue = e.OriginalAppraisedValue;
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
        CurrentHomeowner = Homeowner.From(e.BuyerFullName, e.BuyerEmail, e.BuyerPhone);
    }

    /// <summary>Applies <see cref="ResaleStarted"/>.</summary>
    public void Apply(ResaleStarted e) => OpenResaleId = ResaleId.Create(e.ResaleId);

    /// <summary>Applies <see cref="ResaleClosed"/>.</summary>
    public void Apply(ResaleClosed _) => OpenResaleId = null;

    /// <summary>Applies <see cref="GroundLeaseTerminated"/>.</summary>
    public void Apply(GroundLeaseTerminated _)
    {
        Status = GroundLeaseStatus.Inactive;
    }

}
