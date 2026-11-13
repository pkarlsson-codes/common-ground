using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.MortgageRequests;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using Marten;
using Wolverine.Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Records a homeowner's request to put a new loan on the home (Model Lease Art 8.3).</summary>
/// <param name="MortgageRequestId">The new request's ID, chosen by the caller.</param>
/// <param name="GroundLeaseId">The homeowner's lease.</param>
/// <param name="ReceivedOn">The day the request reached the trust.</param>
/// <param name="Terms">The loan's terms.</param>
/// <param name="ExpectedClosingOn">The day the loan is expected to close.</param>
/// <param name="ClosingCosts">The loan's closing costs, if known.</param>
/// <param name="Appraisal">The loan's appraisal, if it came with the request.</param>
/// <param name="Notes">Anything staff want to note.</param>
/// <param name="ReceivedBy">The staff user recording it.</param>
public sealed record ReceiveMortgageRequest(
    MortgageRequestId MortgageRequestId,
    GroundLeaseId GroundLeaseId,
    DateOnly ReceivedOn,
    MortgageLoanTerms Terms,
    DateOnly ExpectedClosingOn,
    Money? ClosingCosts,
    LoanAppraisal? Appraisal,
    string? Notes,
    UserId ReceivedBy);

/// <summary>What receiving a mortgage request decides from.</summary>
/// <param name="Lease">The homeowner's lease.</param>
/// <param name="PendingOnLease">The lease's pending request, if it has one.</param>
/// <param name="Today">The trust's today.</param>
public sealed record MortgageRequestReceipt(GroundLease Lease, MortgageRequestId? PendingOnLease, DateOnly Today);

/// <summary>Handles <see cref="ReceiveMortgageRequest"/>.</summary>
public static class ReceiveMortgageRequestHandler
{
    /// <summary>Loads the lease, its pending request, and the trust's today.</summary>
    public static async Task<MortgageRequestReceipt> LoadAsync(
        ReceiveMortgageRequest command,
        IDocumentSession session,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(trustClock);

        var leaseId = command.GroundLeaseId.Value;
        var lease = await session.LoadAggregateAsync<GroundLease>(leaseId, cancellationToken).ConfigureAwait(false);
        var pendingStatus = MortgageRequestStatus.Pending.DisplayName;
        var pendingId = await session.Query<MortgageRequestView>()
            .Where(v => v.GroundLeaseId == leaseId && v.Status == pendingStatus)
            .Select(v => v.Id)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
        var today = await trustClock.TodayAsync(session.TenantId, cancellationToken).ConfigureAwait(false);

        return new MortgageRequestReceipt(lease, pendingId is null ? null : MortgageRequestId.Create(pendingId), today);
    }

    /// <summary>Opens the request's stream.</summary>
    public static IStartStream Handle(ReceiveMortgageRequest command, MortgageRequestReceipt receipt)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(receipt);

        var events = MortgageRequest.Receive(
            command.MortgageRequestId,
            receipt.Lease,
            receipt.PendingOnLease,
            command.ReceivedOn,
            command.Terms,
            command.ExpectedClosingOn,
            command.ClosingCosts,
            command.Appraisal,
            command.Notes,
            receipt.Today,
            command.ReceivedBy);

        return MartenOps.StartStream<MortgageRequest>(command.MortgageRequestId.Value, [.. events]);
    }
}
