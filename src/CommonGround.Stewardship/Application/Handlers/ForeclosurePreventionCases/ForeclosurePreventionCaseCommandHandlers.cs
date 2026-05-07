using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.Stewardship.Application.Commands.ForeclosurePreventionCases;
using CommonGround.Stewardship.Domain.ForeclosurePreventionCases;
using Marten;

namespace CommonGround.Stewardship.Application.Handlers.ForeclosurePreventionCases;

#pragma warning disable CS1591
#pragma warning disable CA2007

/// <summary>Handles <see cref="OpenForeclosurePreventionCaseCommand"/>.</summary>
public static class OpenForeclosurePreventionCaseHandler
{
    public static async Task<string> Handle(OpenForeclosurePreventionCaseCommand cmd, IDocumentSession session, CancellationToken ct)
    {
        var fpc = ForeclosurePreventionCase.Open(cmd.GroundLeaseId, ForeclosurePreventionTrigger.ServicerDelinquency, cmd.RecordedDate, cmd.RecordedBy);
        fpc.RecordServicerDelinquency(cmd.ServicerName, cmd.DelinquentMonths, cmd.RecordedDate, cmd.RecordedBy);
        fpc.ValidateInvariants();
        session.Events.StartStream<ForeclosurePreventionCase>(fpc.Id.Value, fpc.UncommittedEvents.ToArray());
        fpc.UncommittedEvents.MarkAsCommitted();
        return fpc.Id.Value;
    }
}

/// <summary>Handles <see cref="RecordServicerDelinquencyCommand"/>.</summary>
public static class RecordServicerDelinquencyHandler
{
    public static async Task Handle(RecordServicerDelinquencyCommand cmd, IDocumentSession session, CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<ForeclosurePreventionCase>(cmd.ForeclosurePreventionCaseId, ct);
        var fpc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(ForeclosurePreventionCase), cmd.ForeclosurePreventionCaseId);
        fpc.RecordServicerDelinquency(cmd.ServicerName, cmd.DelinquentMonths, cmd.RecordedDate, cmd.RecordedBy);
        fpc.ValidateInvariants();
        stream.AppendMany(fpc.UncommittedEvents.ToArray());
        fpc.UncommittedEvents.MarkAsCommitted();
    }
}

/// <summary>Handles <see cref="MakeCounselingReferralCommand"/>.</summary>
public static class MakeCounselingReferralHandler
{
    public static async Task Handle(MakeCounselingReferralCommand cmd, IDocumentSession session, CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<ForeclosurePreventionCase>(cmd.ForeclosurePreventionCaseId, ct);
        var fpc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(ForeclosurePreventionCase), cmd.ForeclosurePreventionCaseId);
        fpc.MakeCounselingReferral(cmd.AgencyName, cmd.ReferralDate, cmd.ReferredBy);
        fpc.ValidateInvariants();
        stream.AppendMany(fpc.UncommittedEvents.ToArray());
        fpc.UncommittedEvents.MarkAsCommitted();
    }
}

/// <summary>Handles <see cref="RecordCounselingOutcomeCommand"/>.</summary>
public static class RecordCounselingOutcomeHandler
{
    public static async Task Handle(RecordCounselingOutcomeCommand cmd, IDocumentSession session, CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<ForeclosurePreventionCase>(cmd.ForeclosurePreventionCaseId, ct);
        var fpc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(ForeclosurePreventionCase), cmd.ForeclosurePreventionCaseId);
        fpc.RecordCounselingOutcome(cmd.ReferralId, cmd.Outcome, cmd.OutcomeDate, cmd.RecordedBy);
        fpc.ValidateInvariants();
        stream.AppendMany(fpc.UncommittedEvents.ToArray());
        fpc.UncommittedEvents.MarkAsCommitted();
    }
}

/// <summary>Handles <see cref="RequestForbearanceCommand"/>.</summary>
public static class RequestForbearanceHandler
{
    public static async Task Handle(RequestForbearanceCommand cmd, IDocumentSession session, CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<ForeclosurePreventionCase>(cmd.ForeclosurePreventionCaseId, ct);
        var fpc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(ForeclosurePreventionCase), cmd.ForeclosurePreventionCaseId);
        fpc.RequestForbearance(cmd.LenderName, cmd.RequestedDate, cmd.RequestedBy);
        fpc.ValidateInvariants();
        stream.AppendMany(fpc.UncommittedEvents.ToArray());
        fpc.UncommittedEvents.MarkAsCommitted();
    }
}

/// <summary>Handles <see cref="RecordForbearanceOutcomeCommand"/>.</summary>
public static class RecordForbearanceOutcomeHandler
{
    public static async Task Handle(RecordForbearanceOutcomeCommand cmd, IDocumentSession session, CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<ForeclosurePreventionCase>(cmd.ForeclosurePreventionCaseId, ct);
        var fpc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(ForeclosurePreventionCase), cmd.ForeclosurePreventionCaseId);
        fpc.RecordForbearanceOutcome(cmd.RequestId, cmd.Outcome, cmd.OutcomeDate, cmd.Notes, cmd.RecordedBy);
        fpc.ValidateInvariants();
        stream.AppendMany(fpc.UncommittedEvents.ToArray());
        fpc.UncommittedEvents.MarkAsCommitted();
    }
}

/// <summary>Handles <see cref="ActivateCurePeriodCommand"/>.</summary>
public static class ActivateCurePeriodHandler
{
    public static async Task Handle(ActivateCurePeriodCommand cmd, IDocumentSession session, CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<ForeclosurePreventionCase>(cmd.ForeclosurePreventionCaseId, ct);
        var fpc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(ForeclosurePreventionCase), cmd.ForeclosurePreventionCaseId);
        fpc.ActivateCurePeriod(cmd.LraId, cmd.ActivationDate, cmd.CurePeriodDays, cmd.ActivatedBy);
        fpc.ValidateInvariants();
        stream.AppendMany(fpc.UncommittedEvents.ToArray());
        fpc.UncommittedEvents.MarkAsCommitted();
    }
}

/// <summary>Handles <see cref="ResolveForeclosurePreventionCaseCommand"/>.</summary>
public static class ResolveForeclosurePreventionCaseHandler
{
    public static async Task Handle(ResolveForeclosurePreventionCaseCommand cmd, IDocumentSession session, CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<ForeclosurePreventionCase>(cmd.ForeclosurePreventionCaseId, ct);
        var fpc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(ForeclosurePreventionCase), cmd.ForeclosurePreventionCaseId);
        fpc.Resolve(cmd.Outcome, cmd.ResolvedDate, cmd.ResolvedBy);
        fpc.ValidateInvariants();
        stream.AppendMany(fpc.UncommittedEvents.ToArray());
        fpc.UncommittedEvents.MarkAsCommitted();
    }
}
