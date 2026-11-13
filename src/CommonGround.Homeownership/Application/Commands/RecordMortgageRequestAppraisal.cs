using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.MortgageRequests;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using JasperFx.Events;
using Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Records the loan's appraisal and closing costs on a request waiting for a decision.</summary>
/// <param name="MortgageRequestId">The request.</param>
/// <param name="Appraisal">The loan's appraisal.</param>
/// <param name="ClosingCosts">The loan's closing costs, or null when not yet known.</param>
/// <param name="RecordedBy">The staff user recording it.</param>
public sealed record RecordMortgageRequestAppraisal(
    MortgageRequestId MortgageRequestId,
    LoanAppraisal Appraisal,
    Money? ClosingCosts,
    UserId RecordedBy);

/// <summary>What recording a request's appraisal decides from.</summary>
/// <param name="Request">The request stream, fetched for writing.</param>
/// <param name="Lease">The request's lease, whose terms price the home.</param>
/// <param name="Today">The trust's today.</param>
public sealed record MortgageRequestAppraisalContext(IEventStream<MortgageRequest> Request, GroundLease Lease, DateOnly Today);

/// <summary>Handles <see cref="RecordMortgageRequestAppraisal"/>.</summary>
public static class RecordMortgageRequestAppraisalHandler
{
    /// <summary>Fetches the request for writing, its lease, and the trust's today.</summary>
    public static async Task<MortgageRequestAppraisalContext> LoadAsync(
        RecordMortgageRequestAppraisal command,
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
        var lease = await session.LoadAggregateAsync<GroundLease>(leaseId.Value, cancellationToken).ConfigureAwait(false);
        var today = await trustClock.TodayAsync(session.TenantId, cancellationToken).ConfigureAwait(false);

        return new MortgageRequestAppraisalContext(request, lease, today);
    }

    /// <summary>Records the appraisal and the debt check it gives.</summary>
    public static void Handle(RecordMortgageRequestAppraisal command, MortgageRequestAppraisalContext context)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(context);

        context.Request.Decide(r => r.RecordAppraisal(
            context.Lease, command.Appraisal, command.ClosingCosts, context.Today, command.RecordedBy));
    }
}
