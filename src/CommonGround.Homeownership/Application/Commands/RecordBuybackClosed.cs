using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.Resales;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using JasperFx.Events;
using Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Records that the trust paid for and took a home it bought back from its homeowner.</summary>
/// <param name="ResaleId">The resale.</param>
/// <param name="ClosingDate">The day the buyback closed.</param>
/// <param name="PricePaid">What the trust paid.</param>
/// <param name="Notes">Anything staff want to note about the closing.</param>
/// <param name="OfferForSale">Whether the home goes to the waiting list now, rather than being held.</param>
/// <param name="HoldReason">Why the trust is holding the home, when it is.</param>
/// <param name="ClosedBy">The staff user recording it.</param>
public sealed record RecordBuybackClosed(
    ResaleId ResaleId,
    DateOnly ClosingDate,
    Money PricePaid,
    string? Notes,
    bool OfferForSale,
    string? HoldReason,
    UserId ClosedBy);

/// <summary>What closing a buyback writes to, and the trust's today.</summary>
/// <param name="Resale">The resale stream, fetched for writing.</param>
/// <param name="Lease">The seller's lease stream, fetched for writing.</param>
/// <param name="Today">The trust's today.</param>
public sealed record BuybackClosing(IEventStream<Resale> Resale, IEventStream<GroundLease> Lease, DateOnly Today);

/// <summary>Handles <see cref="RecordBuybackClosed"/>.</summary>
public static class RecordBuybackClosedHandler
{
    /// <summary>The reason the seller's lease is terminated with.</summary>
    public const string LeaseEndedReason = "Bought by the trust";

    /// <summary>Fetches the resale and its lease for writing, and the trust's today.</summary>
    public static async Task<BuybackClosing> LoadAsync(
        RecordBuybackClosed command,
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
        var lease = await session.Events
            .FetchForWriting<GroundLease>(leaseId.Value, cancellationToken)
            .ConfigureAwait(false);
        var today = await trustClock.TodayAsync(session.TenantId, cancellationToken).ConfigureAwait(false);

        return new BuybackClosing(resale, lease, today);
    }

    /// <summary>Closes the resale, then ends the seller's lease and clears its open resale, in the same session.</summary>
    public static void Handle(RecordBuybackClosed command, BuybackClosing closing)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(closing);

        closing.Resale.Decide(r => r.CloseBuyback(
            command.ClosingDate,
            command.PricePaid,
            command.Notes,
            command.OfferForSale,
            command.HoldReason,
            closing.Today,
            command.ClosedBy));
        closing.Lease.Decide(l => l.Terminate(LeaseEndedReason, command.ClosingDate, command.ClosedBy));
        closing.Lease.Decide(l => l.CloseResale(command.ResaleId));
    }
}
