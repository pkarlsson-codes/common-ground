using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.ImprovementRequests;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using Marten;
using Wolverine.Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Records a homeowner's request to alter or add to the home (Model Lease Art 7.3).</summary>
/// <param name="ImprovementRequestId">The new request's ID, chosen by the caller.</param>
/// <param name="GroundLeaseId">The homeowner's lease.</param>
/// <param name="ReceivedOn">The day the request reached the trust.</param>
/// <param name="Description">What the homeowner wants to do.</param>
/// <param name="EstimatedCost">The homeowner's cost estimate.</param>
/// <param name="ReceivedBy">The staff user recording it.</param>
public sealed record ReceiveImprovementRequest(
    ImprovementRequestId ImprovementRequestId,
    GroundLeaseId GroundLeaseId,
    DateOnly ReceivedOn,
    string Description,
    Money EstimatedCost,
    UserId ReceivedBy);

/// <summary>What receiving an improvement request decides from.</summary>
/// <param name="Lease">The homeowner's lease.</param>
/// <param name="Today">The trust's today.</param>
public sealed record ImprovementRequestReceipt(GroundLease Lease, DateOnly Today);

/// <summary>Handles <see cref="ReceiveImprovementRequest"/>.</summary>
public static class ReceiveImprovementRequestHandler
{
    /// <summary>Loads the lease and the trust's today.</summary>
    public static async Task<ImprovementRequestReceipt> LoadAsync(
        ReceiveImprovementRequest command,
        IDocumentSession session,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(trustClock);

        var lease = await session.LoadAggregateAsync<GroundLease>(command.GroundLeaseId.Value, cancellationToken).ConfigureAwait(false);
        var today = await trustClock.TodayAsync(session.TenantId, cancellationToken).ConfigureAwait(false);

        return new ImprovementRequestReceipt(lease, today);
    }

    /// <summary>Opens the request's stream.</summary>
    public static IStartStream Handle(ReceiveImprovementRequest command, ImprovementRequestReceipt receipt)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(receipt);

        var received = ImprovementRequest.Receive(
            command.ImprovementRequestId,
            receipt.Lease,
            command.ReceivedOn,
            command.Description,
            command.EstimatedCost,
            receipt.Today,
            command.ReceivedBy);

        return MartenOps.StartStream<ImprovementRequest>(command.ImprovementRequestId.Value, received);
    }
}
