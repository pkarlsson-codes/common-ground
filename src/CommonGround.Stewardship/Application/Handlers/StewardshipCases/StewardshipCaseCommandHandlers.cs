using CommonGround.Stewardship.Application.Commands.StewardshipCases;
using CommonGround.Stewardship.Domain.StewardshipCases;
using CommonGround.Stewardship.Domain.StewardshipCases.Events;
using Wolverine.Marten;

namespace CommonGround.Stewardship.Application.Handlers.StewardshipCases;

#pragma warning disable CS1591

/// <summary>Handles <see cref="InitiateOccupancyVerificationCommand"/>.</summary>
public static class InitiateOccupancyVerificationHandler
{
    public static OccupancyVerificationInitiated Handle(InitiateOccupancyVerificationCommand cmd, [WriteAggregate] StewardshipCase sc)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        ArgumentNullException.ThrowIfNull(sc);

        return sc.InitiateOccupancyVerification(cmd.PeriodYear, cmd.SurveyDeadline, cmd.InitiatedBy);
    }
}

/// <summary>Handles <see cref="SubmitOccupancyVerificationCommand"/>.</summary>
public static class SubmitOccupancyVerificationHandler
{
    public static OccupancyVerificationSubmitted Handle(SubmitOccupancyVerificationCommand cmd, [WriteAggregate] StewardshipCase sc)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        ArgumentNullException.ThrowIfNull(sc);

        return sc.SubmitOccupancyVerification(cmd.VerificationId, cmd.IsPrimaryResident, cmd.SubletDisclosed, cmd.MajorChanges, cmd.SubmittedDate);
    }
}

/// <summary>Handles <see cref="VerifyOccupancyCommand"/>.</summary>
public static class VerifyOccupancyHandler
{
    public static OccupancyVerified Handle(VerifyOccupancyCommand cmd, [WriteAggregate] StewardshipCase sc)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        ArgumentNullException.ThrowIfNull(sc);

        return sc.VerifyOccupancy(cmd.VerificationId, cmd.VerifiedDate, cmd.VerifiedBy);
    }
}

/// <summary>Handles <see cref="FailOccupancyCommand"/>.</summary>
public static class FailOccupancyHandler
{
    public static OccupancyVerificationFailed Handle(FailOccupancyCommand cmd, [WriteAggregate] StewardshipCase sc)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        ArgumentNullException.ThrowIfNull(sc);

        return sc.FailOccupancy(cmd.VerificationId, cmd.Reason, cmd.FailedDate, cmd.RecordedBy);
    }
}

/// <summary>Handles <see cref="MarkOccupancyVerificationOverdueCommand"/>.</summary>
public static class MarkOccupancyVerificationOverdueHandler
{
    public static OccupancyVerificationOverdue Handle(MarkOccupancyVerificationOverdueCommand cmd, [WriteAggregate] StewardshipCase sc)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        ArgumentNullException.ThrowIfNull(sc);

        return sc.MarkOccupancyVerificationOverdue(cmd.VerificationId, cmd.Deadline);
    }
}

/// <summary>Handles <see cref="SchedulePropertyInspectionCommand"/>.</summary>
public static class SchedulePropertyInspectionHandler
{
    public static PropertyInspectionScheduled Handle(SchedulePropertyInspectionCommand cmd, [WriteAggregate] StewardshipCase sc)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        ArgumentNullException.ThrowIfNull(sc);

        return sc.SchedulePropertyInspection(cmd.ScheduledDate, cmd.InspectorName, cmd.ScheduledBy);
    }
}

/// <summary>Handles <see cref="CompletePropertyInspectionCommand"/>.</summary>
public static class CompletePropertyInspectionHandler
{
    public static PropertyInspectionCompleted Handle(CompletePropertyInspectionCommand cmd, [WriteAggregate] StewardshipCase sc)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        ArgumentNullException.ThrowIfNull(sc);

        return sc.CompletePropertyInspection(cmd.InspectionId, cmd.OccupancyConfirmed, cmd.ConditionNotes, cmd.CompletedDate, cmd.RecordedBy);
    }
}

/// <summary>Handles <see cref="CancelPropertyInspectionCommand"/>.</summary>
public static class CancelPropertyInspectionHandler
{
    public static PropertyInspectionCancelled Handle(CancelPropertyInspectionCommand cmd, [WriteAggregate] StewardshipCase sc)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        ArgumentNullException.ThrowIfNull(sc);

        return sc.CancelPropertyInspection(cmd.InspectionId, cmd.Reason, cmd.CancelledBy);
    }
}

/// <summary>Handles <see cref="IdentifyViolationCommand"/>.</summary>
public static class IdentifyViolationHandler
{
    public static LeaseViolationIdentified Handle(IdentifyViolationCommand cmd, [WriteAggregate] StewardshipCase sc)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        ArgumentNullException.ThrowIfNull(sc);

        return sc.IdentifyViolation(cmd.Type, cmd.Description, cmd.IdentifiedDate, cmd.IdentifiedBy);
    }
}

/// <summary>Handles <see cref="IssueViolationNoticeCommand"/>.</summary>
public static class IssueViolationNoticeHandler
{
    public static ViolationNoticeIssued Handle(IssueViolationNoticeCommand cmd, [WriteAggregate] StewardshipCase sc)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        ArgumentNullException.ThrowIfNull(sc);

        return sc.IssueViolationNotice(cmd.ViolationId, cmd.NoticeDate, cmd.CureDeadline, cmd.IssuedBy);
    }
}

/// <summary>Handles <see cref="CureViolationCommand"/>.</summary>
public static class CureViolationHandler
{
    public static LeaseViolationCured Handle(CureViolationCommand cmd, [WriteAggregate] StewardshipCase sc)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        ArgumentNullException.ThrowIfNull(sc);

        return sc.CureViolation(cmd.ViolationId, cmd.CuredDate, cmd.VerifiedBy);
    }
}

/// <summary>Handles <see cref="EscalateViolationCommand"/>.</summary>
public static class EscalateViolationHandler
{
    public static LeaseViolationEscalated Handle(EscalateViolationCommand cmd, [WriteAggregate] StewardshipCase sc)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        ArgumentNullException.ThrowIfNull(sc);

        return sc.EscalateViolation(cmd.ViolationId, cmd.Reason, cmd.EscalatedDate, cmd.EscalatedBy);
    }
}

/// <summary>Handles <see cref="CloseStewardshipCaseCommand"/>.</summary>
public static class CloseStewardshipCaseHandler
{
    public static StewardshipCaseClosed Handle(CloseStewardshipCaseCommand cmd, [WriteAggregate] StewardshipCase sc)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        ArgumentNullException.ThrowIfNull(sc);

        return sc.Close(cmd.Reason, cmd.ClosedDate, cmd.ClosedBy);
    }
}
