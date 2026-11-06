using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.Resales;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using JasperFx.Events;
using Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Records that a homeowner who gave notice to sell has decided not to.</summary>
/// <param name="ResaleId">The resale to withdraw.</param>
/// <param name="WithdrawnDate">The day the homeowner withdrew.</param>
/// <param name="Reason">Why the homeowner withdrew.</param>
/// <param name="WithdrawnBy">The staff user recording it.</param>
public sealed record WithdrawIntentToSell(
    ResaleId ResaleId,
    DateOnly WithdrawnDate,
    string Reason,
    UserId WithdrawnBy);

/// <summary>What withdrawing an intent to sell writes to, and the trust's today.</summary>
/// <param name="Resale">The resale stream, fetched for writing.</param>
/// <param name="Lease">The resale's lease stream, fetched for writing.</param>
/// <param name="Today">The trust's today.</param>
public sealed record IntentToSellWithdrawal(IEventStream<Resale> Resale, IEventStream<GroundLease> Lease, DateOnly Today);

/// <summary>Handles <see cref="WithdrawIntentToSell"/>.</summary>
public static class WithdrawIntentToSellHandler
{
    /// <summary>Fetches the resale and its lease for writing, and the trust's today.</summary>
    public static async Task<IntentToSellWithdrawal> LoadAsync(
        WithdrawIntentToSell command,
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

        return new IntentToSellWithdrawal(resale, lease, today);
    }

    /// <summary>Withdraws the resale and closes it on its lease, in the same session.</summary>
    public static void Handle(WithdrawIntentToSell command, IntentToSellWithdrawal withdrawal)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(withdrawal);

        withdrawal.Resale.Decide(r => r.Withdraw(command.WithdrawnDate, command.Reason, withdrawal.Today, command.WithdrawnBy));
        withdrawal.Lease.Decide(l => l.CloseResale(command.ResaleId));
    }
}
