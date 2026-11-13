using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.Mortgages;
using CommonGround.Homeownership.Domain.MortgageRequests;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using JasperFx.Events;
using Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>
/// Records that an approved loan closed: the lease gains it as a permitted mortgage and marks the mortgages it paid off.
/// </summary>
/// <param name="MortgageRequestId">The approved request.</param>
/// <param name="PermittedMortgageId">The new permitted mortgage's ID, chosen by the caller.</param>
/// <param name="ClosedOn">The day the loan closed.</param>
/// <param name="PaidOffMortgageIds">The lease's mortgages the loan paid off.</param>
/// <param name="RecordedBy">The staff user recording it.</param>
public sealed record RecordMortgageLoanClosed(
    MortgageRequestId MortgageRequestId,
    PermittedMortgageId PermittedMortgageId,
    DateOnly ClosedOn,
    IReadOnlyList<PermittedMortgageId> PaidOffMortgageIds,
    UserId RecordedBy);

/// <summary>What recording a closed loan writes to, and the trust's today.</summary>
/// <param name="Request">The request stream, fetched for writing.</param>
/// <param name="Lease">The request's lease stream, fetched for writing.</param>
/// <param name="Today">The trust's today.</param>
public sealed record MortgageLoanClosing(IEventStream<MortgageRequest> Request, IEventStream<GroundLease> Lease, DateOnly Today);

/// <summary>Handles <see cref="RecordMortgageLoanClosed"/>.</summary>
public static class RecordMortgageLoanClosedHandler
{
    /// <summary>Fetches the request and its lease for writing, and the trust's today.</summary>
    public static async Task<MortgageLoanClosing> LoadAsync(
        RecordMortgageLoanClosed command,
        IDocumentSession session,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(trustClock);

        var request = await session.Events
            .FetchForWriting<MortgageRequest>(command.MortgageRequestId.Value, cancellationToken)
            .ConfigureAwait(false);
        var leaseId = request.Aggregate?.GroundLeaseId
            ?? throw new AggregateNotFoundException(typeof(MortgageRequest), command.MortgageRequestId.Value);
        var lease = await session.Events
            .FetchForWriting<GroundLease>(leaseId.Value, cancellationToken)
            .ConfigureAwait(false);
        var today = await trustClock.TodayAsync(session.TenantId, cancellationToken).ConfigureAwait(false);

        return new MortgageLoanClosing(request, lease, today);
    }

    /// <summary>Closes the request, then permits the loan on the lease, in the same session.</summary>
    public static void Handle(RecordMortgageLoanClosed command, MortgageLoanClosing closing)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(closing);

        closing.Request.Decide(r => r.RecordLoanClosed(
            command.ClosedOn, command.PermittedMortgageId, command.PaidOffMortgageIds, closing.Today, command.RecordedBy));
        closing.Lease.Decide(l => l.PermitRequestedMortgage(
            command.PermittedMortgageId, closing.Request.Aggregate!, command.ClosedOn, command.PaidOffMortgageIds, closing.Today, command.RecordedBy));
    }
}
