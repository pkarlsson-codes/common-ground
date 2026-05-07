using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.Stewardship.Application.Commands.StewardshipCases;
using CommonGround.Stewardship.Domain.StewardshipCases;
using Marten;

namespace CommonGround.Stewardship.Application.Handlers.StewardshipCases;

#pragma warning disable CS1591
#pragma warning disable CA2007

/// <summary>Handles <see cref="InitiateOccupancyVerificationCommand"/>.</summary>
public static class InitiateOccupancyVerificationHandler
{
    public static async Task Handle(InitiateOccupancyVerificationCommand cmd, IDocumentSession session, CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<StewardshipCase>(cmd.StewardshipCaseId, ct);
        var sc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(StewardshipCase), cmd.StewardshipCaseId);
        sc.InitiateOccupancyVerification(cmd.PeriodYear, cmd.SurveyDeadline, cmd.InitiatedBy);
        sc.ValidateInvariants();
        stream.AppendMany(sc.UncommittedEvents.ToArray());
        sc.UncommittedEvents.MarkAsCommitted();
    }
}

/// <summary>Handles <see cref="SubmitOccupancyVerificationCommand"/>.</summary>
public static class SubmitOccupancyVerificationHandler
{
    public static async Task Handle(SubmitOccupancyVerificationCommand cmd, IDocumentSession session, CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<StewardshipCase>(cmd.StewardshipCaseId, ct);
        var sc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(StewardshipCase), cmd.StewardshipCaseId);
        sc.SubmitOccupancyVerification(cmd.VerificationId, cmd.IsPrimaryResident, cmd.SubletDisclosed, cmd.MajorChanges, cmd.SubmittedDate);
        sc.ValidateInvariants();
        stream.AppendMany(sc.UncommittedEvents.ToArray());
        sc.UncommittedEvents.MarkAsCommitted();
    }
}

/// <summary>Handles <see cref="VerifyOccupancyCommand"/>.</summary>
public static class VerifyOccupancyHandler
{
    public static async Task Handle(VerifyOccupancyCommand cmd, IDocumentSession session, CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<StewardshipCase>(cmd.StewardshipCaseId, ct);
        var sc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(StewardshipCase), cmd.StewardshipCaseId);
        sc.VerifyOccupancy(cmd.VerificationId, cmd.VerifiedDate, cmd.VerifiedBy);
        sc.ValidateInvariants();
        stream.AppendMany(sc.UncommittedEvents.ToArray());
        sc.UncommittedEvents.MarkAsCommitted();
    }
}

/// <summary>Handles <see cref="FailOccupancyCommand"/>.</summary>
public static class FailOccupancyHandler
{
    public static async Task Handle(FailOccupancyCommand cmd, IDocumentSession session, CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<StewardshipCase>(cmd.StewardshipCaseId, ct);
        var sc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(StewardshipCase), cmd.StewardshipCaseId);
        sc.FailOccupancy(cmd.VerificationId, cmd.Reason, cmd.FailedDate, cmd.RecordedBy);
        sc.ValidateInvariants();
        stream.AppendMany(sc.UncommittedEvents.ToArray());
        sc.UncommittedEvents.MarkAsCommitted();
    }
}

/// <summary>Handles <see cref="MarkOccupancyVerificationOverdueCommand"/>.</summary>
public static class MarkOccupancyVerificationOverdueHandler
{
    public static async Task Handle(MarkOccupancyVerificationOverdueCommand cmd, IDocumentSession session, CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<StewardshipCase>(cmd.StewardshipCaseId, ct);
        var sc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(StewardshipCase), cmd.StewardshipCaseId);
        sc.MarkOccupancyVerificationOverdue(cmd.VerificationId, cmd.Deadline);
        sc.ValidateInvariants();
        stream.AppendMany(sc.UncommittedEvents.ToArray());
        sc.UncommittedEvents.MarkAsCommitted();
    }
}

/// <summary>Handles <see cref="SchedulePropertyInspectionCommand"/>.</summary>
public static class SchedulePropertyInspectionHandler
{
    public static async Task Handle(SchedulePropertyInspectionCommand cmd, IDocumentSession session, CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<StewardshipCase>(cmd.StewardshipCaseId, ct);
        var sc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(StewardshipCase), cmd.StewardshipCaseId);
        sc.SchedulePropertyInspection(cmd.ScheduledDate, cmd.InspectorName, cmd.ScheduledBy);
        sc.ValidateInvariants();
        stream.AppendMany(sc.UncommittedEvents.ToArray());
        sc.UncommittedEvents.MarkAsCommitted();
    }
}

/// <summary>Handles <see cref="CompletePropertyInspectionCommand"/>.</summary>
public static class CompletePropertyInspectionHandler
{
    public static async Task Handle(CompletePropertyInspectionCommand cmd, IDocumentSession session, CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<StewardshipCase>(cmd.StewardshipCaseId, ct);
        var sc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(StewardshipCase), cmd.StewardshipCaseId);
        sc.CompletePropertyInspection(cmd.InspectionId, cmd.OccupancyConfirmed, cmd.ConditionNotes, cmd.CompletedDate, cmd.RecordedBy);
        sc.ValidateInvariants();
        stream.AppendMany(sc.UncommittedEvents.ToArray());
        sc.UncommittedEvents.MarkAsCommitted();
    }
}

/// <summary>Handles <see cref="CancelPropertyInspectionCommand"/>.</summary>
public static class CancelPropertyInspectionHandler
{
    public static async Task Handle(CancelPropertyInspectionCommand cmd, IDocumentSession session, CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<StewardshipCase>(cmd.StewardshipCaseId, ct);
        var sc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(StewardshipCase), cmd.StewardshipCaseId);
        sc.CancelPropertyInspection(cmd.InspectionId, cmd.Reason, cmd.CancelledBy);
        sc.ValidateInvariants();
        stream.AppendMany(sc.UncommittedEvents.ToArray());
        sc.UncommittedEvents.MarkAsCommitted();
    }
}

/// <summary>Handles <see cref="IdentifyViolationCommand"/>.</summary>
public static class IdentifyViolationHandler
{
    public static async Task Handle(IdentifyViolationCommand cmd, IDocumentSession session, CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<StewardshipCase>(cmd.StewardshipCaseId, ct);
        var sc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(StewardshipCase), cmd.StewardshipCaseId);
        sc.IdentifyViolation(cmd.Type, cmd.Description, cmd.IdentifiedDate, cmd.IdentifiedBy);
        sc.ValidateInvariants();
        stream.AppendMany(sc.UncommittedEvents.ToArray());
        sc.UncommittedEvents.MarkAsCommitted();
    }
}

/// <summary>Handles <see cref="IssueViolationNoticeCommand"/>.</summary>
public static class IssueViolationNoticeHandler
{
    public static async Task Handle(IssueViolationNoticeCommand cmd, IDocumentSession session, CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<StewardshipCase>(cmd.StewardshipCaseId, ct);
        var sc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(StewardshipCase), cmd.StewardshipCaseId);
        sc.IssueViolationNotice(cmd.ViolationId, cmd.NoticeDate, cmd.CureDeadline, cmd.IssuedBy);
        sc.ValidateInvariants();
        stream.AppendMany(sc.UncommittedEvents.ToArray());
        sc.UncommittedEvents.MarkAsCommitted();
    }
}

/// <summary>Handles <see cref="CureViolationCommand"/>.</summary>
public static class CureViolationHandler
{
    public static async Task Handle(CureViolationCommand cmd, IDocumentSession session, CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<StewardshipCase>(cmd.StewardshipCaseId, ct);
        var sc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(StewardshipCase), cmd.StewardshipCaseId);
        sc.CureViolation(cmd.ViolationId, cmd.CuredDate, cmd.VerifiedBy);
        sc.ValidateInvariants();
        stream.AppendMany(sc.UncommittedEvents.ToArray());
        sc.UncommittedEvents.MarkAsCommitted();
    }
}

/// <summary>Handles <see cref="EscalateViolationCommand"/>.</summary>
public static class EscalateViolationHandler
{
    public static async Task Handle(EscalateViolationCommand cmd, IDocumentSession session, CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<StewardshipCase>(cmd.StewardshipCaseId, ct);
        var sc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(StewardshipCase), cmd.StewardshipCaseId);
        sc.EscalateViolation(cmd.ViolationId, cmd.Reason, cmd.EscalatedDate, cmd.EscalatedBy);
        sc.ValidateInvariants();
        stream.AppendMany(sc.UncommittedEvents.ToArray());
        sc.UncommittedEvents.MarkAsCommitted();
    }
}

/// <summary>Handles <see cref="CloseStewardshipCaseCommand"/>.</summary>
public static class CloseStewardshipCaseHandler
{
    public static async Task Handle(CloseStewardshipCaseCommand cmd, IDocumentSession session, CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<StewardshipCase>(cmd.StewardshipCaseId, ct);
        var sc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(StewardshipCase), cmd.StewardshipCaseId);
        sc.Close(cmd.Reason, cmd.ClosedDate, cmd.ClosedBy);
        sc.ValidateInvariants();
        stream.AppendMany(sc.UncommittedEvents.ToArray());
        sc.UncommittedEvents.MarkAsCommitted();
    }
}
