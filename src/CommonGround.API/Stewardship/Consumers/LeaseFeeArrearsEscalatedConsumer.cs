using CommonGround.API.Contracts.Stewardship;
using CommonGround.SharedKernel.Domain;
using CommonGround.Stewardship.Domain.ForeclosurePreventionCases;
using Marten;

namespace CommonGround.API.Stewardship.Consumers;

#pragma warning disable SA1600
public sealed class LeaseFeeArrearsEscalatedConsumer(IDocumentSession session)
{
    public void Handle(LeaseFeeArrearsEscalated @event, CancellationToken ct)
    {
        // TODO: Deduplication rule not yet defined. Currently opens a new case every time.
        // See open question in plan: if LeaseFeeArrearsEscalated and RecordServicerDelinquency
        // both arrive for the same ground lease in quick succession, should they open one case or two?
        var fpc = ForeclosurePreventionCase.Open(
            @event.GroundLeaseId,
            ForeclosurePreventionTrigger.LeaseFeeArrears,
            @event.EscalationDate,
            UserId.Create("system"));

        fpc.ValidateInvariants();
        session.Events.StartStream<ForeclosurePreventionCase>(fpc.Id.Value, fpc.UncommittedEvents.ToArray());
        fpc.UncommittedEvents.MarkAsCommitted();
    }
}
