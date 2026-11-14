using CommonGround.Homeownership.Application.Queries;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.ImprovementRequests;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using JasperFx.Events;
using Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Approves the work with a credit the trust agrees to count once the work is confirmed.</summary>
/// <param name="ImprovementRequestId">The request.</param>
/// <param name="ApprovedOn">The day the trust approved it.</param>
/// <param name="PreApprovedCredit">The most the trust will count; zero approves the work and counts nothing.</param>
/// <param name="Notes">Anything staff want to note.</param>
/// <param name="ApprovedBy">The staff user recording it.</param>
public sealed record ApproveImprovementRequest(
    ImprovementRequestId ImprovementRequestId,
    DateOnly ApprovedOn,
    Money PreApprovedCredit,
    string? Notes,
    UserId ApprovedBy);

/// <summary>Refuses the work.</summary>
/// <param name="ImprovementRequestId">The request.</param>
/// <param name="DeniedOn">The day the trust refused it.</param>
/// <param name="Reason">Why, for the denial letter.</param>
/// <param name="DeniedBy">The staff user recording it.</param>
public sealed record DenyImprovementRequest(ImprovementRequestId ImprovementRequestId, DateOnly DeniedOn, string Reason, UserId DeniedBy);

/// <summary>Records the finished work and confirms the credit.</summary>
/// <param name="ImprovementRequestId">The request.</param>
/// <param name="CompletedOn">The day the work finished.</param>
/// <param name="ActualCost">What the work cost.</param>
/// <param name="ReceiptDocumentIds">The receipts, as Documents file IDs. At least one.</param>
/// <param name="ConfirmedCredit">The credit that counts; at most the pre-approved credit.</param>
/// <param name="Notes">Anything staff want to note.</param>
/// <param name="CompletedBy">The staff user recording it.</param>
public sealed record RecordImprovementCompleted(
    ImprovementRequestId ImprovementRequestId,
    DateOnly CompletedOn,
    Money ActualCost,
    IReadOnlyList<string> ReceiptDocumentIds,
    Money ConfirmedCredit,
    string? Notes,
    UserId CompletedBy);

/// <summary>Records that the homeowner dropped the work.</summary>
/// <param name="ImprovementRequestId">The request.</param>
/// <param name="WithdrawnOn">The day the homeowner withdrew it.</param>
/// <param name="Reason">Why the homeowner withdrew it.</param>
/// <param name="WithdrawnBy">The staff user recording it.</param>
public sealed record WithdrawImprovementRequest(ImprovementRequestId ImprovementRequestId, DateOnly WithdrawnOn, string Reason, UserId WithdrawnBy);

/// <summary>What deciding on, completing or withdrawing a request decides from.</summary>
/// <param name="Request">The request stream, fetched for writing.</param>
/// <param name="Today">The trust's today.</param>
public sealed record ImprovementRequestDecision(IEventStream<ImprovementRequest> Request, DateOnly Today)
{
    /// <summary>Fetches the request for writing and the trust's today.</summary>
    public static async Task<ImprovementRequestDecision> LoadAsync(
        ImprovementRequestId requestId,
        IDocumentSession session,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(trustClock);

        var request = await session.Events
            .FetchForWriting<ImprovementRequest>(requestId.Value, cancellationToken)
            .ConfigureAwait(false);
        var today = await trustClock.TodayAsync(session.TenantId, cancellationToken).ConfigureAwait(false);

        return new ImprovementRequestDecision(request, today);
    }
}

/// <summary>What approving a request decides from.</summary>
/// <param name="Request">The request stream, fetched for writing.</param>
/// <param name="LeaseImprovementsCap">The lease's improvements cap, or null when it has none.</param>
/// <param name="CreditOnLease">The confirmed credit the lease already has.</param>
/// <param name="Today">The trust's today.</param>
public sealed record ImprovementApprovalContext(
    IEventStream<ImprovementRequest> Request,
    Money? LeaseImprovementsCap,
    Money CreditOnLease,
    DateOnly Today);

/// <summary>Handles <see cref="ApproveImprovementRequest"/>.</summary>
public static class ApproveImprovementRequestHandler
{
    /// <summary>Fetches the request for writing, its lease's cap and confirmed credit, and the trust's today.</summary>
    public static async Task<ImprovementApprovalContext> LoadAsync(
        ApproveImprovementRequest command,
        IDocumentSession session,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(trustClock);

        var decision = await ImprovementRequestDecision.LoadAsync(command.ImprovementRequestId, session, trustClock, cancellationToken).ConfigureAwait(false);
        var leaseId = decision.Request.Aggregate?.GroundLeaseId
            ?? throw new AggregateNotFoundException(typeof(ImprovementRequest), command.ImprovementRequestId.Value);
        var lease = await session.LoadAggregateAsync<GroundLease>(leaseId.Value, cancellationToken).ConfigureAwait(false);
        var credit = await ConfirmedImprovementCredit.ForLeaseAsync(session, leaseId.Value, cancellationToken).ConfigureAwait(false);
        var cap = lease.ResalePolicyTerms.ImprovementsCap is { } amount ? Money.Create(amount) : null;

        return new ImprovementApprovalContext(decision.Request, cap, credit, decision.Today);
    }

    /// <summary>Approves the request.</summary>
    public static void Handle(ApproveImprovementRequest command, ImprovementApprovalContext context)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(context);

        context.Request.Decide(r => r.Approve(
            command.ApprovedOn, command.PreApprovedCredit, context.LeaseImprovementsCap, context.CreditOnLease,
            command.Notes, context.Today, command.ApprovedBy));
    }
}

/// <summary>Handles <see cref="DenyImprovementRequest"/>.</summary>
public static class DenyImprovementRequestHandler
{
    /// <summary>Fetches the request for writing and the trust's today.</summary>
    public static Task<ImprovementRequestDecision> LoadAsync(
        DenyImprovementRequest command, IDocumentSession session, ITrustClock trustClock, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        return ImprovementRequestDecision.LoadAsync(command.ImprovementRequestId, session, trustClock, cancellationToken);
    }

    /// <summary>Denies the request.</summary>
    public static void Handle(DenyImprovementRequest command, ImprovementRequestDecision decision)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(decision);

        decision.Request.Decide(r => r.Deny(command.DeniedOn, command.Reason, decision.Today, command.DeniedBy));
    }
}

/// <summary>Handles <see cref="RecordImprovementCompleted"/>.</summary>
public static class RecordImprovementCompletedHandler
{
    /// <summary>Fetches the request for writing and the trust's today.</summary>
    public static Task<ImprovementRequestDecision> LoadAsync(
        RecordImprovementCompleted command, IDocumentSession session, ITrustClock trustClock, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        return ImprovementRequestDecision.LoadAsync(command.ImprovementRequestId, session, trustClock, cancellationToken);
    }

    /// <summary>Records the finished work.</summary>
    public static void Handle(RecordImprovementCompleted command, ImprovementRequestDecision decision)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(decision);

        decision.Request.Decide(r => r.RecordCompleted(
            command.CompletedOn, command.ActualCost, command.ReceiptDocumentIds, command.ConfirmedCredit,
            command.Notes, decision.Today, command.CompletedBy));
    }
}

/// <summary>Handles <see cref="WithdrawImprovementRequest"/>.</summary>
public static class WithdrawImprovementRequestHandler
{
    /// <summary>Fetches the request for writing and the trust's today.</summary>
    public static Task<ImprovementRequestDecision> LoadAsync(
        WithdrawImprovementRequest command, IDocumentSession session, ITrustClock trustClock, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        return ImprovementRequestDecision.LoadAsync(command.ImprovementRequestId, session, trustClock, cancellationToken);
    }

    /// <summary>Withdraws the request.</summary>
    public static void Handle(WithdrawImprovementRequest command, ImprovementRequestDecision decision)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(decision);

        decision.Request.Decide(r => r.Withdraw(command.WithdrawnOn, command.Reason, decision.Today, command.WithdrawnBy));
    }
}
