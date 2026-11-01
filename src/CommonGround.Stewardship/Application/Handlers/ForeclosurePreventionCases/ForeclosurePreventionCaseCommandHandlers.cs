using CommonGround.Stewardship.Application.Commands.ForeclosurePreventionCases;
using CommonGround.Stewardship.Domain.ForeclosurePreventionCases;
using CommonGround.Stewardship.Domain.ForeclosurePreventionCases.Events;
using Wolverine.Marten;

namespace CommonGround.Stewardship.Application.Handlers.ForeclosurePreventionCases;

#pragma warning disable CS1591

/// <summary>Handles <see cref="OpenForeclosurePreventionCaseCommand"/>.</summary>
public static class OpenForeclosurePreventionCaseHandler
{
    public static IStartStream Handle(OpenForeclosurePreventionCaseCommand cmd)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var opened = ForeclosurePreventionCase.Open(
            cmd.ForeclosurePreventionCaseId, cmd.GroundLeaseId, ForeclosurePreventionTrigger.ServicerDelinquency, cmd.RecordedDate, cmd.RecordedBy);
        var delinquency = ForeclosurePreventionCase.Create(opened)
            .RecordServicerDelinquency(cmd.ServicerName, cmd.DelinquentMonths, cmd.RecordedDate, cmd.RecordedBy);

        return MartenOps.StartStream<ForeclosurePreventionCase>(cmd.ForeclosurePreventionCaseId.Value, opened, delinquency);
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
