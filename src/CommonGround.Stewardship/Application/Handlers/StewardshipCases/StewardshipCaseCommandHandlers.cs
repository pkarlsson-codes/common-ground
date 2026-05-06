using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.Stewardship.Application.Commands.StewardshipCases;
using CommonGround.Stewardship.Domain.StewardshipCases;
using Marten;

namespace CommonGround.Stewardship.Application.Handlers.StewardshipCases;

#pragma warning disable CS1591
#pragma warning disable CA2007
public sealed class StewardshipCaseCommandHandler(IDocumentSession session)
{
    public async Task Handle(InitiateOccupancyVerificationCommand cmd, CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<StewardshipCase>(cmd.StewardshipCaseId, ct);
        var sc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(StewardshipCase), cmd.StewardshipCaseId);
        sc.InitiateOccupancyVerification(cmd.PeriodYear, cmd.SurveyDeadline, UserId.Create(cmd.InitiatedBy));
        sc.ValidateInvariants();
        stream.AppendMany(sc.UncommittedEvents.ToArray());
        sc.UncommittedEvents.MarkAsCommitted();
    }

    public async Task Handle(SubmitOccupancyVerificationCommand cmd, CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<StewardshipCase>(cmd.StewardshipCaseId, ct);
        var sc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(StewardshipCase), cmd.StewardshipCaseId);
        sc.SubmitOccupancyVerification(cmd.VerificationId, cmd.IsPrimaryResident, cmd.SubletDisclosed, cmd.MajorChanges, cmd.SubmittedDate);
        sc.ValidateInvariants();
        stream.AppendMany(sc.UncommittedEvents.ToArray());
        sc.UncommittedEvents.MarkAsCommitted();
    }

    public async Task Handle(VerifyOccupancyCommand cmd, CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<StewardshipCase>(cmd.StewardshipCaseId, ct);
        var sc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(StewardshipCase), cmd.StewardshipCaseId);
        sc.VerifyOccupancy(cmd.VerificationId, cmd.VerifiedDate, UserId.Create(cmd.VerifiedBy));
        sc.ValidateInvariants();
        stream.AppendMany(sc.UncommittedEvents.ToArray());
        sc.UncommittedEvents.MarkAsCommitted();
    }

    public async Task Handle(FailOccupancyCommand cmd, CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<StewardshipCase>(cmd.StewardshipCaseId, ct);
        var sc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(StewardshipCase), cmd.StewardshipCaseId);
        sc.FailOccupancy(cmd.VerificationId, cmd.Reason, cmd.FailedDate, UserId.Create(cmd.RecordedBy));
        sc.ValidateInvariants();
        stream.AppendMany(sc.UncommittedEvents.ToArray());
        sc.UncommittedEvents.MarkAsCommitted();
    }

    public async Task Handle(MarkOccupancyVerificationOverdueCommand cmd, CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<StewardshipCase>(cmd.StewardshipCaseId, ct);
        var sc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(StewardshipCase), cmd.StewardshipCaseId);
        sc.MarkOccupancyVerificationOverdue(cmd.VerificationId, cmd.Deadline);
        sc.ValidateInvariants();
        stream.AppendMany(sc.UncommittedEvents.ToArray());
        sc.UncommittedEvents.MarkAsCommitted();
    }

    public async Task Handle(SchedulePropertyInspectionCommand cmd, CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<StewardshipCase>(cmd.StewardshipCaseId, ct);
        var sc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(StewardshipCase), cmd.StewardshipCaseId);
        sc.SchedulePropertyInspection(cmd.ScheduledDate, cmd.InspectorName, UserId.Create(cmd.ScheduledBy));
        sc.ValidateInvariants();
        stream.AppendMany(sc.UncommittedEvents.ToArray());
        sc.UncommittedEvents.MarkAsCommitted();
    }

    public async Task Handle(CompletePropertyInspectionCommand cmd, CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<StewardshipCase>(cmd.StewardshipCaseId, ct);
        var sc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(StewardshipCase), cmd.StewardshipCaseId);
        sc.CompletePropertyInspection(cmd.InspectionId, cmd.OccupancyConfirmed, cmd.ConditionNotes, cmd.CompletedDate, UserId.Create(cmd.RecordedBy));
        sc.ValidateInvariants();
        stream.AppendMany(sc.UncommittedEvents.ToArray());
        sc.UncommittedEvents.MarkAsCommitted();
    }

    public async Task Handle(CancelPropertyInspectionCommand cmd, CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<StewardshipCase>(cmd.StewardshipCaseId, ct);
        var sc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(StewardshipCase), cmd.StewardshipCaseId);
        sc.CancelPropertyInspection(cmd.InspectionId, cmd.Reason, UserId.Create(cmd.CancelledBy));
        sc.ValidateInvariants();
        stream.AppendMany(sc.UncommittedEvents.ToArray());
        sc.UncommittedEvents.MarkAsCommitted();
    }

    public async Task Handle(IdentifyViolationCommand cmd, CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<StewardshipCase>(cmd.StewardshipCaseId, ct);
        var sc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(StewardshipCase), cmd.StewardshipCaseId);
        sc.IdentifyViolation(cmd.Type, cmd.Description, cmd.IdentifiedDate, UserId.Create(cmd.IdentifiedBy));
        sc.ValidateInvariants();
        stream.AppendMany(sc.UncommittedEvents.ToArray());
        sc.UncommittedEvents.MarkAsCommitted();
    }

    public async Task Handle(IssueViolationNoticeCommand cmd, CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<StewardshipCase>(cmd.StewardshipCaseId, ct);
        var sc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(StewardshipCase), cmd.StewardshipCaseId);
        sc.IssueViolationNotice(cmd.ViolationId, cmd.NoticeDate, cmd.CureDeadline, UserId.Create(cmd.IssuedBy));
        sc.ValidateInvariants();
        stream.AppendMany(sc.UncommittedEvents.ToArray());
        sc.UncommittedEvents.MarkAsCommitted();
    }

    public async Task Handle(CureViolationCommand cmd, CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<StewardshipCase>(cmd.StewardshipCaseId, ct);
        var sc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(StewardshipCase), cmd.StewardshipCaseId);
        sc.CureViolation(cmd.ViolationId, cmd.CuredDate, UserId.Create(cmd.VerifiedBy));
        sc.ValidateInvariants();
        stream.AppendMany(sc.UncommittedEvents.ToArray());
        sc.UncommittedEvents.MarkAsCommitted();
    }

    public async Task Handle(EscalateViolationCommand cmd, CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<StewardshipCase>(cmd.StewardshipCaseId, ct);
        var sc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(StewardshipCase), cmd.StewardshipCaseId);
        sc.EscalateViolation(cmd.ViolationId, cmd.Reason, cmd.EscalatedDate, UserId.Create(cmd.EscalatedBy));
        sc.ValidateInvariants();
        stream.AppendMany(sc.UncommittedEvents.ToArray());
        sc.UncommittedEvents.MarkAsCommitted();
    }

    public async Task Handle(CloseStewardshipCaseCommand cmd, CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<StewardshipCase>(cmd.StewardshipCaseId, ct);
        var sc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(StewardshipCase), cmd.StewardshipCaseId);
        sc.Close(cmd.Reason, cmd.ClosedDate, UserId.Create(cmd.ClosedBy));
        sc.ValidateInvariants();
        stream.AppendMany(sc.UncommittedEvents.ToArray());
        sc.UncommittedEvents.MarkAsCommitted();
    }
}
