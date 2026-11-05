using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.SharedKernel.Domain;
using Marten;
using Wolverine.Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Creates a ground lease directly, rather than from a purchase closing.</summary>
public sealed record CreateGroundLease(
    GroundLeaseId GroundLeaseId,
    string LandParcelId,
    string UnitId,
    Homeowner Homeowner,
    DateOnly LeaseStartDate,
    int TermYears,
    string CovenantTerms,
    ResalePolicyTerms ResalePolicyTerms,
    UserId CreatedBy);

/// <summary>Handles <see cref="CreateGroundLease"/>.</summary>
public static class CreateGroundLeaseHandler
{
    private static readonly string[] TerminalPhaseNames =
    [
        WorkflowPhase.Closed.DisplayName,
        WorkflowPhase.Failed.DisplayName,
        WorkflowPhase.Withdrawn.DisplayName,
    ];

    /// <summary>Finds what the home is already committed to: an active lease, an open purchase or an open selection.</summary>
    public static async Task<HomeCommitments> LoadAsync(CreateGroundLease command, IQuerySession session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(session);

        var active = GroundLeaseStatus.Active.DisplayName;
        var activeLease = await session.Query<GroundLeaseListItemView>()
            .Where(v => v.HomeId == command.UnitId && v.Status == active)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
        var openPurchase = await session.Query<PurchaseWorkflowListItemView>()
            .Where(v => v.HomeId == command.UnitId && !TerminalPhaseNames.Contains(v.Phase))
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
        var openSelection = await session.Query<HomeSelectionView>()
            .Where(v => v.HomeId == command.UnitId && v.Open)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return HomeCommitments.Create(activeLease?.Id, openPurchase?.Id, openSelection?.Id);
    }

    /// <summary>Starts the new stream.</summary>
    public static IStartStream Handle(CreateGroundLease command, HomeCommitments homeCommitments)
    {
        ArgumentNullException.ThrowIfNull(command);

        var started = GroundLease.Establish(
            command.GroundLeaseId,
            command.LandParcelId,
            command.UnitId,
            command.Homeowner,
            command.LeaseStartDate,
            command.TermYears,
            command.CovenantTerms,
            command.ResalePolicyTerms,
            homeCommitments,
            command.CreatedBy);

        return MartenOps.StartStream<GroundLease>(command.GroundLeaseId.Value, started);
    }
}
