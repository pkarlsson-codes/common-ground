using CommonGround.Stewardship.Application.Commands.ForeclosurePreventionCases;
using CommonGround.Stewardship.Domain.ForeclosurePreventionCases;
using CommonGround.Stewardship.Domain.ForeclosurePreventionCases.Events;
using CommonGround.Stewardship.Infrastructure.Persistence;
using Marten;
using Wolverine.Marten;

namespace CommonGround.Stewardship.Application.Handlers.ForeclosurePreventionCases;

#pragma warning disable CS1591

/// <summary>The lease's unresolved foreclosure prevention case, if it has one.</summary>
/// <param name="Id">The case's ID, or null.</param>
public sealed record UnresolvedForeclosurePreventionCase(ForeclosurePreventionCaseId? Id)
{
    /// <summary>Finds the lease's unresolved case. The case view is projected inline, so it is current.</summary>
    public static async Task<UnresolvedForeclosurePreventionCase> FindAsync(IQuerySession session, string groundLeaseId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(session);

        var id = await session.Query<ForeclosurePreventionCaseView>()
            .Where(x => x.GroundLeaseId == groundLeaseId && x.Status != nameof(ForeclosurePreventionCaseStatus.Resolved))
            .Select(x => x.Id)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

        return new UnresolvedForeclosurePreventionCase(id is null ? null : ForeclosurePreventionCaseId.Create(id));
    }
}

/// <summary>Handles <see cref="OpenForeclosurePreventionCaseCommand"/>.</summary>
public static class OpenForeclosurePreventionCaseHandler
{
    public static Task<UnresolvedForeclosurePreventionCase> LoadAsync(OpenForeclosurePreventionCaseCommand cmd, IQuerySession session, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        return UnresolvedForeclosurePreventionCase.FindAsync(session, cmd.GroundLeaseId, ct);
    }

    public static IStartStream Handle(OpenForeclosurePreventionCaseCommand cmd, UnresolvedForeclosurePreventionCase unresolved)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        ArgumentNullException.ThrowIfNull(unresolved);

        var opened = ForeclosurePreventionCase.Open(
            cmd.ForeclosurePreventionCaseId, cmd.GroundLeaseId, ForeclosurePreventionTrigger.ServicerDelinquency, cmd.RecordedDate, unresolved.Id, cmd.RecordedBy);
        var delinquency = ForeclosurePreventionCase.Create(opened)
            .RecordServicerDelinquency(cmd.ServicerName, cmd.DelinquentMonths, cmd.RecordedDate, cmd.RecordedBy);

        return MartenOps.StartStream<ForeclosurePreventionCase>(cmd.ForeclosurePreventionCaseId.Value, opened, delinquency);
    }
}

/// <summary>Handles <see cref="OpenForeclosurePreventionCaseForLeaseDefaultCommand"/>.</summary>
public static class OpenForeclosurePreventionCaseForLeaseDefaultHandler
{
    public static Task<UnresolvedForeclosurePreventionCase> LoadAsync(OpenForeclosurePreventionCaseForLeaseDefaultCommand cmd, IQuerySession session, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        return UnresolvedForeclosurePreventionCase.FindAsync(session, cmd.GroundLeaseId, ct);
    }

    public static IStartStream Handle(OpenForeclosurePreventionCaseForLeaseDefaultCommand cmd, UnresolvedForeclosurePreventionCase unresolved)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        ArgumentNullException.ThrowIfNull(unresolved);

        var events = ForeclosurePreventionCase.OpenForLeaseDefault(
            cmd.ForeclosurePreventionCaseId, cmd.GroundLeaseId, cmd.UnpaidDefault, cmd.OpenedDate, unresolved.Id, cmd.OpenedBy);

        return MartenOps.StartStream<ForeclosurePreventionCase>(cmd.ForeclosurePreventionCaseId.Value, [.. events]);
    }
}

/// <summary>Handles <see cref="RecordServicerDelinquencyCommand"/>.</summary>
public static class RecordServicerDelinquencyHandler
{
    public static ServicerDelinquencyRecorded Handle(RecordServicerDelinquencyCommand cmd, [WriteAggregate] ForeclosurePreventionCase fpc)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        ArgumentNullException.ThrowIfNull(fpc);

        return fpc.RecordServicerDelinquency(cmd.ServicerName, cmd.DelinquentMonths, cmd.RecordedDate, cmd.RecordedBy);
    }
}

/// <summary>Handles <see cref="MakeCounselingReferralCommand"/>.</summary>
public static class MakeCounselingReferralHandler
{
    public static CounselingReferralMade Handle(MakeCounselingReferralCommand cmd, [WriteAggregate] ForeclosurePreventionCase fpc)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        ArgumentNullException.ThrowIfNull(fpc);

        return fpc.MakeCounselingReferral(cmd.AgencyName, cmd.ReferralDate, cmd.ReferredBy);
    }
}

/// <summary>Handles <see cref="RecordCounselingOutcomeCommand"/>.</summary>
public static class RecordCounselingOutcomeHandler
{
    public static CounselingOutcomeRecorded Handle(RecordCounselingOutcomeCommand cmd, [WriteAggregate] ForeclosurePreventionCase fpc)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        ArgumentNullException.ThrowIfNull(fpc);

        return fpc.RecordCounselingOutcome(cmd.ReferralId, cmd.Outcome, cmd.OutcomeDate, cmd.RecordedBy);
    }
}

/// <summary>Handles <see cref="RequestForbearanceCommand"/>.</summary>
public static class RequestForbearanceHandler
{
    public static ForbearanceRequested Handle(RequestForbearanceCommand cmd, [WriteAggregate] ForeclosurePreventionCase fpc)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        ArgumentNullException.ThrowIfNull(fpc);

        return fpc.RequestForbearance(cmd.LenderName, cmd.RequestedDate, cmd.RequestedBy);
    }
}

/// <summary>Handles <see cref="RecordForbearanceOutcomeCommand"/>.</summary>
public static class RecordForbearanceOutcomeHandler
{
    public static ForbearanceOutcomeRecorded Handle(RecordForbearanceOutcomeCommand cmd, [WriteAggregate] ForeclosurePreventionCase fpc)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        ArgumentNullException.ThrowIfNull(fpc);

        return fpc.RecordForbearanceOutcome(cmd.RequestId, cmd.Outcome, cmd.OutcomeDate, cmd.Notes, cmd.RecordedBy);
    }
}

/// <summary>Handles <see cref="ActivateCurePeriodCommand"/>.</summary>
public static class ActivateCurePeriodHandler
{
    public static CurePeriodActivated Handle(ActivateCurePeriodCommand cmd, [WriteAggregate] ForeclosurePreventionCase fpc)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        ArgumentNullException.ThrowIfNull(fpc);

        return fpc.ActivateCurePeriod(cmd.LraId, cmd.ActivationDate, cmd.CurePeriodDays, cmd.ActivatedBy);
    }
}

/// <summary>Handles <see cref="ResolveForeclosurePreventionCaseCommand"/>.</summary>
public static class ResolveForeclosurePreventionCaseHandler
{
    public static ForeclosurePreventionCaseResolved Handle(ResolveForeclosurePreventionCaseCommand cmd, [WriteAggregate] ForeclosurePreventionCase fpc)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        ArgumentNullException.ThrowIfNull(fpc);

        return fpc.Resolve(cmd.Outcome, cmd.ResolvedDate, cmd.ResolvedBy);
    }
}
