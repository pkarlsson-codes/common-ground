using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.Stewardship.Application.Commands.ForeclosurePreventionCases;
using CommonGround.Stewardship.Domain.ForeclosurePreventionCases;
using Marten;

namespace CommonGround.Stewardship.Application.Handlers.ForeclosurePreventionCases;

#pragma warning disable CS1591
#pragma warning disable CA2007
public sealed class ForeclosurePreventionCaseCommandHandler(IDocumentSession session)
{
    public async Task<string> Handle(OpenForeclosurePreventionCaseCommand cmd, CancellationToken ct)
    {
        var fpc = ForeclosurePreventionCase.Open(cmd.GroundLeaseId, ForeclosurePreventionTrigger.ServicerDelinquency, cmd.RecordedDate, UserId.Create(cmd.RecordedBy));
        fpc.RecordServicerDelinquency(cmd.ServicerName, cmd.DelinquentMonths, cmd.RecordedDate, UserId.Create(cmd.RecordedBy));
        fpc.ValidateInvariants();
        session.Events.StartStream<ForeclosurePreventionCase>(fpc.Id.Value, fpc.UncommittedEvents.ToArray());
        fpc.UncommittedEvents.MarkAsCommitted();
        return fpc.Id.Value;
    }

    public async Task Handle(RecordServicerDelinquencyCommand cmd, CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<ForeclosurePreventionCase>(cmd.ForeclosurePreventionCaseId, ct);
        var fpc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(ForeclosurePreventionCase), cmd.ForeclosurePreventionCaseId);
        fpc.RecordServicerDelinquency(cmd.ServicerName, cmd.DelinquentMonths, cmd.RecordedDate, UserId.Create(cmd.RecordedBy));
        fpc.ValidateInvariants();
        stream.AppendMany(fpc.UncommittedEvents.ToArray());
        fpc.UncommittedEvents.MarkAsCommitted();
    }

    public async Task Handle(MakeCounselingReferralCommand cmd, CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<ForeclosurePreventionCase>(cmd.ForeclosurePreventionCaseId, ct);
        var fpc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(ForeclosurePreventionCase), cmd.ForeclosurePreventionCaseId);
        fpc.MakeCounselingReferral(cmd.AgencyName, cmd.ReferralDate, UserId.Create(cmd.ReferredBy));
        fpc.ValidateInvariants();
        stream.AppendMany(fpc.UncommittedEvents.ToArray());
        fpc.UncommittedEvents.MarkAsCommitted();
    }

    public async Task Handle(RecordCounselingOutcomeCommand cmd, CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<ForeclosurePreventionCase>(cmd.ForeclosurePreventionCaseId, ct);
        var fpc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(ForeclosurePreventionCase), cmd.ForeclosurePreventionCaseId);
        fpc.RecordCounselingOutcome(cmd.ReferralId, cmd.Outcome, cmd.OutcomeDate, UserId.Create(cmd.RecordedBy));
        fpc.ValidateInvariants();
        stream.AppendMany(fpc.UncommittedEvents.ToArray());
        fpc.UncommittedEvents.MarkAsCommitted();
    }

    public async Task Handle(RequestForbearanceCommand cmd, CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<ForeclosurePreventionCase>(cmd.ForeclosurePreventionCaseId, ct);
        var fpc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(ForeclosurePreventionCase), cmd.ForeclosurePreventionCaseId);
        fpc.RequestForbearance(cmd.LenderName, cmd.RequestedDate, UserId.Create(cmd.RequestedBy));
        fpc.ValidateInvariants();
        stream.AppendMany(fpc.UncommittedEvents.ToArray());
        fpc.UncommittedEvents.MarkAsCommitted();
    }

    public async Task Handle(RecordForbearanceOutcomeCommand cmd, CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<ForeclosurePreventionCase>(cmd.ForeclosurePreventionCaseId, ct);
        var fpc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(ForeclosurePreventionCase), cmd.ForeclosurePreventionCaseId);
        fpc.RecordForbearanceOutcome(cmd.RequestId, cmd.Outcome, cmd.OutcomeDate, cmd.Notes, UserId.Create(cmd.RecordedBy));
        fpc.ValidateInvariants();
        stream.AppendMany(fpc.UncommittedEvents.ToArray());
        fpc.UncommittedEvents.MarkAsCommitted();
    }

    public async Task Handle(ActivateCurePeriodCommand cmd, CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<ForeclosurePreventionCase>(cmd.ForeclosurePreventionCaseId, ct);
        var fpc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(ForeclosurePreventionCase), cmd.ForeclosurePreventionCaseId);
        fpc.ActivateCurePeriod(cmd.LraId, cmd.ActivationDate, cmd.CurePeriodDays, UserId.Create(cmd.ActivatedBy));
        fpc.ValidateInvariants();
        stream.AppendMany(fpc.UncommittedEvents.ToArray());
        fpc.UncommittedEvents.MarkAsCommitted();
    }

    public async Task Handle(ResolveForeclosurePreventionCaseCommand cmd, CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<ForeclosurePreventionCase>(cmd.ForeclosurePreventionCaseId, ct);
        var fpc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(ForeclosurePreventionCase), cmd.ForeclosurePreventionCaseId);
        fpc.Resolve(cmd.Outcome, cmd.ResolvedDate, UserId.Create(cmd.ResolvedBy));
        fpc.ValidateInvariants();
        stream.AppendMany(fpc.UncommittedEvents.ToArray());
        fpc.UncommittedEvents.MarkAsCommitted();
    }
}
