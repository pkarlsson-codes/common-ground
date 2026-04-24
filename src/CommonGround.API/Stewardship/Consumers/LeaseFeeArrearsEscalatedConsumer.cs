using CommonGround.API.Contracts.Stewardship;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Persistence;
using CommonGround.Stewardship.Domain.ForeclosurePreventionCases;

namespace CommonGround.API.Stewardship.Consumers;

/// <summary>Handles lease fee arrears escalation events by opening prevention cases.</summary>
public sealed class LeaseFeeArrearsEscalatedConsumer(
    IForeclosurePreventionCaseRepository repository,
    IUnitOfWork unitOfWork)
{
    /// <summary>Opens a foreclosure prevention case for escalated lease fee arrears.</summary>
    /// <param name="event">The lease fee arrears escalation event.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Handle(LeaseFeeArrearsEscalated @event, CancellationToken ct)
    {
        // TODO: Deduplication rule not yet defined. Currently opens a new case every time.
        // See open question in plan: if LeaseFeeArrearsEscalated and RecordServicerDelinquency
        // both arrive for the same ground lease in quick succession, should they open one case or two?
        var fpc = ForeclosurePreventionCase.Open(
            @event.GroundLeaseId,
            ForeclosurePreventionTrigger.LeaseFeeArrears,
            @event.EscalationDate,
            UserId.Create("system"));

        repository.Stage(fpc);
        await unitOfWork.CommitAsync(ct).ConfigureAwait(false);
    }
}
