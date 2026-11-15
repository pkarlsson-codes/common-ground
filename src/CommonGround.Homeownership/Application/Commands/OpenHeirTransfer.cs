using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.HeirTransfers;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using Marten;
using Wolverine.Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Opens a transfer of a deceased homeowner's home to an heir (Model Lease Art 10.3).</summary>
/// <param name="HeirTransferId">The new transfer's ID, chosen by the caller.</param>
/// <param name="GroundLeaseId">The lease the home is held on.</param>
/// <param name="DiedOn">The day the homeowner died.</param>
/// <param name="NoticeReceivedOn">The day the executor told the trust.</param>
/// <param name="NotifiedBy">Who told the trust.</param>
/// <param name="NotifiedByContact">How to reach them, if given.</param>
/// <param name="Notes">Anything staff want to note.</param>
/// <param name="OpenedBy">The staff user opening it.</param>
public sealed record OpenHeirTransfer(
    HeirTransferId HeirTransferId,
    GroundLeaseId GroundLeaseId,
    DateOnly DiedOn,
    DateOnly NoticeReceivedOn,
    string NotifiedBy,
    string? NotifiedByContact,
    string? Notes,
    UserId OpenedBy);

/// <summary>What opening a transfer decides from.</summary>
/// <param name="Lease">The lease the home is held on.</param>
/// <param name="OpenTransferId">The lease's open transfer, if it has one.</param>
/// <param name="Today">The trust's today.</param>
public sealed record HeirTransferOpening(GroundLease Lease, HeirTransferId? OpenTransferId, DateOnly Today);

/// <summary>Handles <see cref="OpenHeirTransfer"/>.</summary>
public static class OpenHeirTransferHandler
{
    /// <summary>Loads the lease, its open transfer if any, and the trust's today.</summary>
    public static async Task<HeirTransferOpening> LoadAsync(
        OpenHeirTransfer command,
        IDocumentSession session,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(trustClock);

        var lease = await session.LoadAggregateAsync<GroundLease>(command.GroundLeaseId.Value, cancellationToken).ConfigureAwait(false);
        var open = HeirTransferView.OpenStatuses;
        var leaseId = command.GroundLeaseId.Value;
        var openTransfer = await session.Query<HeirTransferView>()
            .Where(v => v.GroundLeaseId == leaseId && open.Contains(v.Status))
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
        var today = await trustClock.TodayAsync(session.TenantId, cancellationToken).ConfigureAwait(false);

        return new HeirTransferOpening(lease, openTransfer is null ? null : HeirTransferId.Create(openTransfer.Id), today);
    }

    /// <summary>Opens the transfer's stream.</summary>
    public static IStartStream Handle(OpenHeirTransfer command, HeirTransferOpening opening)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(opening);

        var opened = HeirTransfer.Open(
            command.HeirTransferId,
            opening.Lease,
            opening.OpenTransferId,
            command.DiedOn,
            command.NoticeReceivedOn,
            command.NotifiedBy,
            command.NotifiedByContact,
            command.Notes,
            opening.Today,
            command.OpenedBy);

        return MartenOps.StartStream<HeirTransfer>(command.HeirTransferId.Value, opened);
    }
}
