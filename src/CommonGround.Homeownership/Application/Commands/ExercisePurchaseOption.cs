using CommonGround.Homeownership.Domain.Resales;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using JasperFx.Events;
using Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Records that the trust will buy a home at the purchase option price.</summary>
/// <param name="ResaleId">The resale.</param>
/// <param name="ExercisedDate">The day the trust decided.</param>
/// <param name="Notes">Anything staff want to note about the decision.</param>
/// <param name="ExercisedBy">The staff user recording it.</param>
public sealed record ExercisePurchaseOption(ResaleId ResaleId, DateOnly ExercisedDate, string? Notes, UserId ExercisedBy);

/// <summary>What exercising the option decides from.</summary>
/// <param name="Resale">The resale stream, fetched for writing.</param>
/// <param name="Today">The trust's today.</param>
public sealed record PurchaseOptionDecision(IEventStream<Resale> Resale, DateOnly Today);

/// <summary>Handles <see cref="ExercisePurchaseOption"/>.</summary>
public static class ExercisePurchaseOptionHandler
{
    /// <summary>Fetches the resale for writing and the trust's today.</summary>
    public static async Task<PurchaseOptionDecision> LoadAsync(
        ExercisePurchaseOption command,
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
        var today = await trustClock.TodayAsync(session.TenantId, cancellationToken).ConfigureAwait(false);

        return new PurchaseOptionDecision(resale, today);
    }

    /// <summary>Records the decision.</summary>
    public static void Handle(ExercisePurchaseOption command, PurchaseOptionDecision decision)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(decision);

        decision.Resale.Decide(r => r.ExerciseOption(command.ExercisedDate, command.Notes, decision.Today, command.ExercisedBy));
    }
}
