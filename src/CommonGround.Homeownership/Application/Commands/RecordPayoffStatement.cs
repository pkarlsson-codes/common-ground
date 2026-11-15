using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.Resales;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Records that a payoff statement was printed for a resale's closing agent.</summary>
/// <param name="ResaleId">The resale.</param>
/// <param name="GoodThrough">The day the figures are worked out for.</param>
/// <param name="RecordedBy">The staff user who printed it.</param>
public sealed record RecordPayoffStatement(ResaleId ResaleId, DateOnly GoodThrough, UserId RecordedBy);

/// <summary>Handles <see cref="RecordPayoffStatement"/>.</summary>
public static class RecordPayoffStatementHandler
{
    /// <summary>Fetches the resale for writing, its lease, and the trust's today.</summary>
    public static async Task<ResaleAppraisalContext> LoadAsync(
        RecordPayoffStatement command,
        IDocumentSession session,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(trustClock);

        var resale = await session.Events
            .FetchForWriting<Resale>(command.ResaleId.Value, cancellationToken)
            .ConfigureAwait(false);
        var leaseId = resale.Aggregate?.GroundLeaseId
            ?? throw new AggregateNotFoundException(typeof(Resale), command.ResaleId.Value);
        var lease = await session.LoadAggregateAsync<GroundLease>(leaseId.Value, cancellationToken).ConfigureAwait(false);
        var today = await trustClock.TodayAsync(session.TenantId, cancellationToken).ConfigureAwait(false);

        return new ResaleAppraisalContext(resale, lease, today);
    }

    /// <summary>Records the print on the resale.</summary>
    public static void Handle(RecordPayoffStatement command, ResaleAppraisalContext context)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(context);

        context.Resale.Decide(r => r.RecordPayoffStatement(context.Lease, command.GoodThrough, context.Today, command.RecordedBy));
    }
}
