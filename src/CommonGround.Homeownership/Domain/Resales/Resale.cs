using CommonGround.Homeownership.Application.Errors;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.Resales.Events;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.Resales;

/// <summary>
/// The seller's side of a home changing hands: from the homeowner's notice of intent to sell, through the
/// trust's purchase option window. The lease it belongs to allows one open resale at a time.
/// </summary>
public sealed class Resale : AggregateRoot<ResaleId>
{
#pragma warning disable CS8618 // State is set by Create from the stream's first event.
    private Resale() { }
#pragma warning restore CS8618

    /// <summary>Gets the lease of the homeowner selling.</summary>
    public GroundLeaseId GroundLeaseId { get; private set; }

    /// <summary>Gets the home being sold.</summary>
    public string HomeId { get; private set; }

    /// <summary>Gets the day the homeowner gave notice.</summary>
    public DateOnly NoticeDate { get; private set; }

    /// <summary>Gets the last day of the trust's purchase option.</summary>
    public DateOnly OptionWindowEndsOn { get; private set; }

    /// <summary>Gets where the resale stands.</summary>
    public ResaleStatus Status { get; private set; }

    /// <summary>Gets the latest appraisal, if one has been recorded.</summary>
    public ResaleAppraisal? Appraisal { get; private set; }

    /// <summary>Gets the day the trust decided to buy the home, if it has.</summary>
    public DateOnly? OptionExercisedOn { get; private set; }

    /// <summary>
    /// Opens a resale from a homeowner's notice of intent to sell. The trust's option window runs for the
    /// lease's option window days from the notice date. The lease decides whether a resale may open; see
    /// <see cref="GroundLease.StartResale"/>.
    /// </summary>
    public static IntentToSellRecorded RecordIntentToSell(
        ResaleId id,
        GroundLease lease,
        DateOnly noticeDate,
        NoticeChannel channel,
        string? notes,
        DateOnly today,
        UserId recordedBy)
    {
        ArgumentNullException.ThrowIfNull(lease);
        ArgumentNullException.ThrowIfNull(channel);

        if (noticeDate > today)
            throw BusinessRuleViolation.Resale.RecordIntentToSell.NoticeDateInFuture(noticeDate, today);

        return new IntentToSellRecorded(
            id.Value,
            lease.Id.Value,
            lease.LandParcelId,
            lease.UnitId,
            lease.CurrentHomeowner?.FullName.Value,
            noticeDate,
            channel,
            string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            noticeDate.AddDays(lease.OptionWindowDays),
            recordedBy.Value);
    }

    /// <summary>
    /// Records the appraisal the lease requires, working out the formula price and the purchase option price from
    /// the lease's terms. A later appraisal replaces it, until the trust has decided.
    /// </summary>
    public ResaleAppraisalRecorded RecordAppraisal(
        GroundLease lease,
        decimal appraisedValue,
        DateOnly appraisalDate,
        string appraiserName,
        decimal? approvedImprovements,
        decimal? currentIndexValue,
        DateOnly today,
        UserId recordedBy)
    {
        ArgumentNullException.ThrowIfNull(lease);

        if (Status != ResaleStatus.Open)
            throw BusinessRuleViolation.Resale.RecordAppraisal.NotOpen(Status);

        if (OptionExercisedOn is not null)
            throw BusinessRuleViolation.Resale.RecordAppraisal.OptionExercised();

        if (lease.OriginalAppraisedValue is not { } originalAppraisedValue)
            throw BusinessRuleViolation.Resale.RecordAppraisal.LeaseFiguresMissing();

        if (appraisedValue <= 0)
            throw BusinessRuleViolation.Resale.RecordAppraisal.AppraisedValueNonPositive(appraisedValue);

        if (string.IsNullOrWhiteSpace(appraiserName))
            throw BusinessRuleViolation.Resale.RecordAppraisal.AppraiserRequired();

        if (appraisalDate < NoticeDate)
            throw BusinessRuleViolation.Resale.RecordAppraisal.AppraisalDateBeforeNotice(appraisalDate, NoticeDate);

        if (appraisalDate > today)
            throw BusinessRuleViolation.Resale.RecordAppraisal.AppraisalDateInFuture(appraisalDate, today);

        if (approvedImprovements < 0)
            throw BusinessRuleViolation.Resale.RecordAppraisal.ApprovedImprovementsNegative(approvedImprovements.Value);

        if (currentIndexValue <= 0)
            throw BusinessRuleViolation.Resale.RecordAppraisal.CurrentIndexValueNonPositive(currentIndexValue.Value);

        var price = ResalePrice.Calculate(
            lease.ResalePolicyTerms,
            lease.BasePrice,
            originalAppraisedValue,
            lease.LeaseStartDate,
            appraisedValue,
            appraisalDate,
            approvedImprovements,
            currentIndexValue);

        return new ResaleAppraisalRecorded(
            Id.Value,
            appraisedValue,
            appraisalDate,
            appraiserName.Trim(),
            approvedImprovements,
            currentIndexValue,
            price.FormulaPrice,
            price.PurchaseOptionPrice,
            recordedBy.Value);
    }

    /// <summary>Records that the trust will buy the home at the purchase option price.</summary>
    public PurchaseOptionExercised ExerciseOption(DateOnly exercisedDate, string? notes, DateOnly today, UserId exercisedBy)
    {
        if (Status != ResaleStatus.Open)
            throw BusinessRuleViolation.Resale.ExerciseOption.NotOpen(Status);

        if (OptionExercisedOn is not null)
            throw BusinessRuleViolation.Resale.ExerciseOption.AlreadyExercised();

        if (Appraisal is null)
            throw BusinessRuleViolation.Resale.ExerciseOption.AppraisalRequired();

        if (exercisedDate < Appraisal.AppraisalDate)
            throw BusinessRuleViolation.Resale.ExerciseOption.ExercisedDateBeforeAppraisal(exercisedDate, Appraisal.AppraisalDate);

        if (exercisedDate > today)
            throw BusinessRuleViolation.Resale.ExerciseOption.ExercisedDateInFuture(exercisedDate, today);

        if (exercisedDate > OptionWindowEndsOn)
            throw BusinessRuleViolation.Resale.ExerciseOption.OptionWindowEnded(OptionWindowEndsOn);

        return new PurchaseOptionExercised(Id.Value, exercisedDate, string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(), exercisedBy.Value);
    }

    /// <summary>
    /// Records that the trust paid for and took the home. The caller ends the seller's lease with it.
    /// The home goes to the waiting list, or the trust holds it with a reason.
    /// </summary>
    public BuybackClosed CloseBuyback(
        DateOnly closingDate,
        decimal pricePaid,
        string? notes,
        bool offerForSale,
        string? holdReason,
        DateOnly today,
        UserId closedBy)
    {
        if (Status != ResaleStatus.Open)
            throw BusinessRuleViolation.Resale.CloseBuyback.NotOpen(Status);

        if (OptionExercisedOn is not { } exercisedOn || Appraisal is null)
            throw BusinessRuleViolation.Resale.CloseBuyback.OptionNotExercised();

        if (closingDate < exercisedOn)
            throw BusinessRuleViolation.Resale.CloseBuyback.ClosingDateBeforeExercised(closingDate, exercisedOn);

        if (closingDate > today)
            throw BusinessRuleViolation.Resale.CloseBuyback.ClosingDateInFuture(closingDate, today);

        if (pricePaid <= 0)
            throw BusinessRuleViolation.Resale.CloseBuyback.PricePaidNonPositive(pricePaid);

        if (pricePaid > Appraisal.PurchaseOptionPrice)
            throw BusinessRuleViolation.Resale.CloseBuyback.PricePaidAboveOptionPrice(pricePaid, Appraisal.PurchaseOptionPrice);

        if (!offerForSale && string.IsNullOrWhiteSpace(holdReason))
            throw BusinessRuleViolation.Resale.CloseBuyback.HoldReasonRequired();

        return new BuybackClosed(
            Id.Value,
            GroundLeaseId.Value,
            HomeId,
            closingDate,
            pricePaid,
            string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            offerForSale,
            offerForSale ? null : holdReason!.Trim(),
            closedBy.Value);
    }

    /// <summary>Withdraws the resale because the homeowner decided not to sell.</summary>
    public ResaleWithdrawn Withdraw(DateOnly withdrawnDate, string reason, DateOnly today, UserId withdrawnBy)
    {
        if (Status != ResaleStatus.Open)
            throw BusinessRuleViolation.Resale.Withdraw.NotOpen(Status);

        if (OptionExercisedOn is not null)
            throw BusinessRuleViolation.Resale.Withdraw.OptionExercised();

        if (string.IsNullOrWhiteSpace(reason))
            throw BusinessRuleViolation.Resale.Withdraw.ReasonRequired();

        if (withdrawnDate < NoticeDate)
            throw BusinessRuleViolation.Resale.Withdraw.WithdrawnDateBeforeNotice(withdrawnDate, NoticeDate);

        if (withdrawnDate > today)
            throw BusinessRuleViolation.Resale.Withdraw.WithdrawnDateInFuture(withdrawnDate, today);

        return new ResaleWithdrawn(Id.Value, GroundLeaseId.Value, HomeId, withdrawnDate, reason.Trim(), withdrawnBy.Value);
    }

    /// <summary>Evolves a new resale from its stream's first event.</summary>
    public static Resale Create(IntentToSellRecorded e)
    {
        ArgumentNullException.ThrowIfNull(e);

        return new Resale
        {
            Id = ResaleId.Create(e.AggregateId),
            GroundLeaseId = GroundLeaseId.Create(e.GroundLeaseId),
            HomeId = e.HomeId,
            NoticeDate = e.NoticeDate,
            OptionWindowEndsOn = e.OptionWindowEndsOn,
            Status = ResaleStatus.Open,
        };
    }

    /// <summary>Applies <see cref="ResaleAppraisalRecorded"/>.</summary>
    public void Apply(ResaleAppraisalRecorded e) =>
        Appraisal = new ResaleAppraisal(e.AppraisedValue, e.AppraisalDate, e.FormulaPrice, e.PurchaseOptionPrice);

    /// <summary>Applies <see cref="PurchaseOptionExercised"/>.</summary>
    public void Apply(PurchaseOptionExercised e) => OptionExercisedOn = e.ExercisedDate;

    /// <summary>Applies <see cref="BuybackClosed"/>.</summary>
    public void Apply(BuybackClosed _) => Status = ResaleStatus.Closed;

    /// <summary>Applies <see cref="ResaleWithdrawn"/>.</summary>
    public void Apply(ResaleWithdrawn _) => Status = ResaleStatus.Withdrawn;
}
