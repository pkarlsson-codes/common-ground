using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.Resales;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using JasperFx.Events;
using Marten;
using Wolverine.Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Records that a homeowner has given notice they intend to sell, opening a resale on their lease.</summary>
/// <param name="ResaleId">The new resale's ID, chosen by the caller.</param>
/// <param name="GroundLeaseId">The homeowner's lease.</param>
/// <param name="NoticeDate">The day the homeowner gave notice.</param>
/// <param name="Channel">How the notice reached the trust.</param>
/// <param name="Notes">Anything staff want to note about the notice.</param>
/// <param name="RecordedBy">The staff user recording it.</param>
public sealed record RecordIntentToSell(
    ResaleId ResaleId,
    GroundLeaseId GroundLeaseId,
    DateOnly NoticeDate,
    NoticeChannel Channel,
    string? Notes,
    UserId RecordedBy);

/// <summary>What recording an intent to sell decides from.</summary>
/// <param name="Lease">The homeowner's lease stream, fetched for writing.</param>
/// <param name="Today">The trust's today.</param>
public sealed record IntentToSellStart(IEventStream<GroundLease> Lease, DateOnly Today);

/// <summary>Handles <see cref="RecordIntentToSell"/>.</summary>
public static class RecordIntentToSellHandler
{
    /// <summary>Fetches the lease for writing and the trust's today.</summary>
    public static async Task<IntentToSellStart> LoadAsync(
        RecordIntentToSell command,
        IDocumentSession session,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(trustClock);

        var lease = await session.Events
            .FetchForWriting<GroundLease>(command.GroundLeaseId.Value, cancellationToken)
            .ConfigureAwait(false);
        var today = await trustClock.TodayAsync(session.TenantId, cancellationToken).ConfigureAwait(false);

        return new IntentToSellStart(lease, today);
    }

    /// <summary>Starts the resale on the lease, then opens the resale's stream, in the same session.</summary>
    public static IStartStream Handle(RecordIntentToSell command, IntentToSellStart start)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(start);

        // Decide throws if the lease doesn't exist, so the fetched aggregate is there below.
        start.Lease.Decide(l => l.StartResale(command.ResaleId));

        var recorded = Resale.RecordIntentToSell(
            command.ResaleId,
            start.Lease.Aggregate!,
            command.NoticeDate,
            command.Channel,
            command.Notes,
            start.Today,
            command.RecordedBy);

        return MartenOps.StartStream<Resale>(command.ResaleId.Value, recorded);
    }
}
