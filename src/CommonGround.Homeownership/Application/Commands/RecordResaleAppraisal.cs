using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.Resales;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using JasperFx.Events;
using Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Records the appraisal the lease requires after a homeowner's notice to sell.</summary>
/// <param name="ResaleId">The resale being appraised.</param>
/// <param name="AppraisedValue">The home's appraised market value.</param>
/// <param name="AppraisalDate">The day of the appraisal.</param>
/// <param name="AppraiserName">Who appraised the home.</param>
/// <param name="ApprovedImprovements">The approved improvements the formula credits, if it credits any.</param>
/// <param name="CurrentIndexValue">The current index value, when the lease's formula is index based.</param>
/// <param name="RecordedBy">The staff user recording it.</param>
public sealed record RecordResaleAppraisal(
    ResaleId ResaleId,
    Money AppraisedValue,
    DateOnly AppraisalDate,
    string AppraiserName,
    Money? ApprovedImprovements,
    decimal? CurrentIndexValue,
    UserId RecordedBy);

/// <summary>What recording a resale appraisal decides from.</summary>
/// <param name="Resale">The resale stream, fetched for writing.</param>
/// <param name="Lease">The resale's lease, whose terms price the home.</param>
/// <param name="Today">The trust's today.</param>
public sealed record ResaleAppraisalContext(IEventStream<Resale> Resale, GroundLease Lease, DateOnly Today);

/// <summary>Handles <see cref="RecordResaleAppraisal"/>.</summary>
public static class RecordResaleAppraisalHandler
{
    /// <summary>Fetches the resale for writing, its lease, and the trust's today.</summary>
    public static async Task<ResaleAppraisalContext> LoadAsync(
        RecordResaleAppraisal command,
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

    /// <summary>Records the appraisal and the prices it sets.</summary>
    public static void Handle(RecordResaleAppraisal command, ResaleAppraisalContext context)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(context);

        context.Resale.Decide(r => r.RecordAppraisal(
            context.Lease,
            command.AppraisedValue,
            command.AppraisalDate,
            command.AppraiserName,
            command.ApprovedImprovements,
            command.CurrentIndexValue,
            context.Today,
            command.RecordedBy));
    }
}
