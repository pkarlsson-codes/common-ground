using CommonGround.Homeownership.Application.Errors;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.ImprovementRequests.Events;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.ImprovementRequests;

/// <summary>
/// A homeowner's request to alter or add to the home (Model Lease Art 7.3). The trust replies in writing, approving
/// the work with a pre-approved credit or denying it. Once the work is finished and its receipts are in, the trust
/// confirms the credit, which then counts toward the lease's improvements at resale. A lease may have any number of
/// requests open at once.
/// </summary>
public sealed class ImprovementRequest : AggregateRoot<ImprovementRequestId>
{
    /// <summary>The days the lease gives the trust to reply, counted from receipt (Art 7.3).</summary>
    public const int ReplyDays = 14;

#pragma warning disable CS8618 // State is set by Create from the stream's first event.
    private ImprovementRequest() { }
#pragma warning restore CS8618

    /// <summary>Gets the homeowner's lease.</summary>
    public GroundLeaseId GroundLeaseId { get; private set; }

    /// <summary>Gets where the request stands.</summary>
    public ImprovementRequestStatus Status { get; private set; }

    /// <summary>Gets the day the request reached the trust.</summary>
    public DateOnly ReceivedOn { get; private set; }

    /// <summary>Gets the homeowner's cost estimate.</summary>
    public Money EstimatedCost { get; private set; }

    /// <summary>Gets the day the trust approved the work, if it has.</summary>
    public DateOnly? ApprovedOn { get; private set; }

    /// <summary>Gets the most the trust agreed to count, once approved.</summary>
    public Money? PreApprovedCredit { get; private set; }

    /// <summary>
    /// Records a homeowner's request to alter or add to the home. Refused unless the lease is active, for a received
    /// date in the future or before the lease started, or an estimated cost that isn't above zero.
    /// </summary>
    /// <param name="id">The new request's ID.</param>
    /// <param name="lease">The homeowner's lease.</param>
    /// <param name="receivedOn">The day the request reached the trust.</param>
    /// <param name="description">What the homeowner wants to do.</param>
    /// <param name="estimatedCost">The homeowner's estimate of the cost.</param>
    /// <param name="today">The trust's today.</param>
    /// <param name="receivedBy">The staff user recording it.</param>
    public static ImprovementRequestReceived Receive(
        ImprovementRequestId id,
        GroundLease lease,
        DateOnly receivedOn,
        string description,
        Money estimatedCost,
        DateOnly today,
        UserId receivedBy)
    {
        ArgumentNullException.ThrowIfNull(lease);
        ArgumentNullException.ThrowIfNull(estimatedCost);

        if (lease.Status != GroundLeaseStatus.Active)
            throw BusinessRuleViolation.ImprovementRequest.Receive.LeaseNotActive(lease.Status);

        if (string.IsNullOrWhiteSpace(description))
            throw BusinessRuleViolation.ImprovementRequest.Receive.DescriptionRequired();

        if (!estimatedCost.IsPositive)
            throw BusinessRuleViolation.ImprovementRequest.Receive.EstimatedCostNotPositive(estimatedCost.Amount);

        if (receivedOn > today)
            throw BusinessRuleViolation.ImprovementRequest.Receive.ReceivedOnInFuture(receivedOn, today);

        if (receivedOn < lease.LeaseStartDate)
            throw BusinessRuleViolation.ImprovementRequest.Receive.ReceivedOnBeforeLeaseStart(receivedOn, lease.LeaseStartDate);

        return new ImprovementRequestReceived(
            id.Value,
            lease.Id.Value,
            lease.UnitId,
            lease.CurrentHomeowner?.FullName.Value,
            receivedOn,
            description.Trim(),
            estimatedCost,
            receivedBy.Value);
    }

    /// <summary>
    /// Approves the work with a credit the trust agrees to count once the work is confirmed. The request keeps the
    /// lease's cap and the credit the lease already had. A credit that takes the total past the cap is allowed: the cap
    /// applies at resale. Refused for a credit below zero or above the estimated cost, or an approved date before
    /// receipt or in the future.
    /// </summary>
    /// <param name="approvedOn">The day the trust approved it.</param>
    /// <param name="preApprovedCredit">The most the trust will count for this work.</param>
    /// <param name="leaseImprovementsCap">The lease's improvements cap, or null when it has none.</param>
    /// <param name="creditOnLease">The confirmed credit the lease already has.</param>
    /// <param name="notes">Anything staff want to note.</param>
    /// <param name="today">The trust's today.</param>
    /// <param name="approvedBy">The staff user recording it.</param>
    public ImprovementRequestApproved Approve(
        DateOnly approvedOn,
        Money preApprovedCredit,
        Money? leaseImprovementsCap,
        Money creditOnLease,
        string? notes,
        DateOnly today,
        UserId approvedBy)
    {
        ArgumentNullException.ThrowIfNull(preApprovedCredit);
        ArgumentNullException.ThrowIfNull(creditOnLease);

        if (Status != ImprovementRequestStatus.Waiting)
            throw BusinessRuleViolation.ImprovementRequest.Approve.NotWaiting(Status);

        if (preApprovedCredit.IsNegative || preApprovedCredit > EstimatedCost)
            throw BusinessRuleViolation.ImprovementRequest.Approve.CreditOutOfRange(preApprovedCredit.Amount, EstimatedCost.Amount);

        CheckDate(approvedOn, ReceivedOn, today,
            BusinessRuleViolation.ImprovementRequest.Approve.ApprovedOnBeforeReceived,
            BusinessRuleViolation.ImprovementRequest.Approve.ApprovedOnInFuture);

        return new ImprovementRequestApproved(
            Id.Value,
            GroundLeaseId.Value,
            approvedOn,
            preApprovedCredit,
            leaseImprovementsCap,
            creditOnLease,
            Trimmed(notes),
            approvedBy.Value);
    }

    /// <summary>Refuses the work, with the reason the denial letter gives. Refused without a reason.</summary>
    public ImprovementRequestDenied Deny(DateOnly deniedOn, string reason, DateOnly today, UserId deniedBy)
    {
        if (Status != ImprovementRequestStatus.Waiting)
            throw BusinessRuleViolation.ImprovementRequest.Deny.NotWaiting(Status);

        if (string.IsNullOrWhiteSpace(reason))
            throw BusinessRuleViolation.ImprovementRequest.Deny.ReasonRequired();

        CheckDate(deniedOn, ReceivedOn, today,
            BusinessRuleViolation.ImprovementRequest.Deny.DeniedOnBeforeReceived,
            BusinessRuleViolation.ImprovementRequest.Deny.DeniedOnInFuture);

        return new ImprovementRequestDenied(Id.Value, GroundLeaseId.Value, deniedOn, reason.Trim(), deniedBy.Value);
    }

    /// <summary>
    /// Records the finished work and confirms the credit, which counts from this moment. Refused unless the request is
    /// approved, without a receipt, for an actual cost that isn't above zero, a confirmed credit below zero or above the
    /// pre-approved credit, or a completed date before the approval or in the future.
    /// </summary>
    public ImprovementWorkCompleted RecordCompleted(
        DateOnly completedOn,
        Money actualCost,
        IReadOnlyList<string> receiptDocumentIds,
        Money confirmedCredit,
        string? notes,
        DateOnly today,
        UserId completedBy)
    {
        ArgumentNullException.ThrowIfNull(actualCost);
        ArgumentNullException.ThrowIfNull(receiptDocumentIds);
        ArgumentNullException.ThrowIfNull(confirmedCredit);

        if (Status != ImprovementRequestStatus.Approved || ApprovedOn is not { } approvedOn || PreApprovedCredit is not { } preApproved)
            throw BusinessRuleViolation.ImprovementRequest.RecordCompleted.NotApproved(Status);

        if (receiptDocumentIds.Count == 0 || receiptDocumentIds.Any(string.IsNullOrWhiteSpace))
            throw BusinessRuleViolation.ImprovementRequest.RecordCompleted.ReceiptsRequired();

        if (!actualCost.IsPositive)
            throw BusinessRuleViolation.ImprovementRequest.RecordCompleted.ActualCostNotPositive(actualCost.Amount);

        if (confirmedCredit.IsNegative || confirmedCredit > preApproved)
            throw BusinessRuleViolation.ImprovementRequest.RecordCompleted.CreditOutOfRange(confirmedCredit.Amount, preApproved.Amount);

        CheckDate(completedOn, approvedOn, today,
            BusinessRuleViolation.ImprovementRequest.RecordCompleted.CompletedOnBeforeApproved,
            BusinessRuleViolation.ImprovementRequest.RecordCompleted.CompletedOnInFuture);

        return new ImprovementWorkCompleted(
            Id.Value,
            GroundLeaseId.Value,
            completedOn,
            actualCost,
            [.. receiptDocumentIds.Select(d => d.Trim()).Distinct()],
            confirmedCredit,
            Trimmed(notes),
            completedBy.Value);
    }

    /// <summary>
    /// Records that the homeowner dropped the work, while it waits for a decision or after approval and before it is
    /// recorded finished. Refused without a reason.
    /// </summary>
    public ImprovementRequestWithdrawn Withdraw(DateOnly withdrawnOn, string reason, DateOnly today, UserId withdrawnBy)
    {
        if (Status != ImprovementRequestStatus.Waiting && Status != ImprovementRequestStatus.Approved)
            throw BusinessRuleViolation.ImprovementRequest.Withdraw.NotOpen(Status);

        if (string.IsNullOrWhiteSpace(reason))
            throw BusinessRuleViolation.ImprovementRequest.Withdraw.ReasonRequired();

        CheckDate(withdrawnOn, ReceivedOn, today,
            BusinessRuleViolation.ImprovementRequest.Withdraw.WithdrawnOnBeforeReceived,
            BusinessRuleViolation.ImprovementRequest.Withdraw.WithdrawnOnInFuture);

        return new ImprovementRequestWithdrawn(Id.Value, GroundLeaseId.Value, withdrawnOn, reason.Trim(), withdrawnBy.Value);
    }

    private static string? Trimmed(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();

    private static void CheckDate(
        DateOnly date,
        DateOnly notBefore,
        DateOnly today,
        Func<DateOnly, DateOnly, Exception> beforeStart,
        Func<DateOnly, DateOnly, Exception> inFuture)
    {
        if (date < notBefore)
            throw beforeStart(date, notBefore);

        if (date > today)
            throw inFuture(date, today);
    }

    /// <summary>Evolves a new request from its stream's first event.</summary>
    public static ImprovementRequest Create(ImprovementRequestReceived e)
    {
        ArgumentNullException.ThrowIfNull(e);

        return new ImprovementRequest
        {
            Id = ImprovementRequestId.Create(e.AggregateId),
            GroundLeaseId = GroundLeaseId.Create(e.GroundLeaseId),
            Status = ImprovementRequestStatus.Waiting,
            ReceivedOn = e.ReceivedOn,
            EstimatedCost = e.EstimatedCost,
        };
    }

    /// <summary>Applies <see cref="ImprovementRequestApproved"/>.</summary>
    public void Apply(ImprovementRequestApproved e)
    {
        Status = ImprovementRequestStatus.Approved;
        ApprovedOn = e.ApprovedOn;
        PreApprovedCredit = e.PreApprovedCredit;
    }

    /// <summary>Applies <see cref="ImprovementRequestDenied"/>.</summary>
    public void Apply(ImprovementRequestDenied _) => Status = ImprovementRequestStatus.Denied;

    /// <summary>Applies <see cref="ImprovementWorkCompleted"/>.</summary>
    public void Apply(ImprovementWorkCompleted _) => Status = ImprovementRequestStatus.Completed;

    /// <summary>Applies <see cref="ImprovementRequestWithdrawn"/>.</summary>
    public void Apply(ImprovementRequestWithdrawn _) => Status = ImprovementRequestStatus.Withdrawn;
}
