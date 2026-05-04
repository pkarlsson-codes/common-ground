using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Persistence;
using CommonGround.Stewardship.Application.Commands.ForeclosurePreventionCases;
using CommonGround.Stewardship.Domain.ForeclosurePreventionCases;

namespace CommonGround.Stewardship.Application.Handlers.ForeclosurePreventionCases;

/// <summary>
/// Handles commands for foreclosure prevention cases.
/// </summary>
public sealed class ForeclosurePreventionCaseCommandHandler(
    IForeclosurePreventionCaseRepository repository,
    IUnitOfWork unitOfWork)
{
    /// <summary>
    /// Handles the OpenForeclosurePreventionCaseCommand.
    /// </summary>
    public async Task<string> Handle(OpenForeclosurePreventionCaseCommand cmd, CancellationToken ct)
    {
        var fpc = ForeclosurePreventionCase.Open(
            cmd.GroundLeaseId,
            ForeclosurePreventionTrigger.ServicerDelinquency,
            cmd.RecordedDate,
            UserId.Create(cmd.RecordedBy));

        fpc.RecordServicerDelinquency(cmd.ServicerName, cmd.DelinquentMonths, cmd.RecordedDate, UserId.Create(cmd.RecordedBy));

        repository.Stage(fpc);
        await unitOfWork.CommitAsync(ct).ConfigureAwait(false);

        return fpc.Id.Value;
    }

    /// <summary>
    /// Handles the RecordServicerDelinquencyCommand.
    /// </summary>
    public async Task Handle(RecordServicerDelinquencyCommand cmd, CancellationToken ct)
    {
        var fpc = await repository.GetByIdOrThrowAsync(ForeclosurePreventionCaseId.Create(cmd.ForeclosurePreventionCaseId), ct).ConfigureAwait(false);
        fpc.RecordServicerDelinquency(cmd.ServicerName, cmd.DelinquentMonths, cmd.RecordedDate, UserId.Create(cmd.RecordedBy));
        repository.Stage(fpc);
        await unitOfWork.CommitAsync(ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the MakeCounselingReferralCommand.
    /// </summary>
    public async Task Handle(MakeCounselingReferralCommand cmd, CancellationToken ct)
    {
        var fpc = await repository.GetByIdOrThrowAsync(ForeclosurePreventionCaseId.Create(cmd.ForeclosurePreventionCaseId), ct).ConfigureAwait(false);
        fpc.MakeCounselingReferral(cmd.AgencyName, cmd.ReferralDate, UserId.Create(cmd.ReferredBy));
        repository.Stage(fpc);
        await unitOfWork.CommitAsync(ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the RecordCounselingOutcomeCommand.
    /// </summary>
    public async Task Handle(RecordCounselingOutcomeCommand cmd, CancellationToken ct)
    {
        var fpc = await repository.GetByIdOrThrowAsync(ForeclosurePreventionCaseId.Create(cmd.ForeclosurePreventionCaseId), ct).ConfigureAwait(false);
        fpc.RecordCounselingOutcome(cmd.ReferralId, cmd.Outcome, cmd.OutcomeDate, UserId.Create(cmd.RecordedBy));
        repository.Stage(fpc);
        await unitOfWork.CommitAsync(ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the RequestForbearanceCommand.
    /// </summary>
    public async Task Handle(RequestForbearanceCommand cmd, CancellationToken ct)
    {
        var fpc = await repository.GetByIdOrThrowAsync(ForeclosurePreventionCaseId.Create(cmd.ForeclosurePreventionCaseId), ct).ConfigureAwait(false);
        fpc.RequestForbearance(cmd.LenderName, cmd.RequestedDate, UserId.Create(cmd.RequestedBy));
        repository.Stage(fpc);
        await unitOfWork.CommitAsync(ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the RecordForbearanceOutcomeCommand.
    /// </summary>
    public async Task Handle(RecordForbearanceOutcomeCommand cmd, CancellationToken ct)
    {
        var fpc = await repository.GetByIdOrThrowAsync(ForeclosurePreventionCaseId.Create(cmd.ForeclosurePreventionCaseId), ct).ConfigureAwait(false);
        fpc.RecordForbearanceOutcome(cmd.RequestId, cmd.Outcome, cmd.OutcomeDate, cmd.Notes, UserId.Create(cmd.RecordedBy));
        repository.Stage(fpc);
        await unitOfWork.CommitAsync(ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the ActivateCurePeriodCommand.
    /// </summary>
    public async Task Handle(ActivateCurePeriodCommand cmd, CancellationToken ct)
    {
        var fpc = await repository.GetByIdOrThrowAsync(ForeclosurePreventionCaseId.Create(cmd.ForeclosurePreventionCaseId), ct).ConfigureAwait(false);
        fpc.ActivateCurePeriod(cmd.LraId, cmd.ActivationDate, cmd.CurePeriodDays, UserId.Create(cmd.ActivatedBy));
        repository.Stage(fpc);
        await unitOfWork.CommitAsync(ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the ResolveForeclosurePreventionCaseCommand.
    /// </summary>
    public async Task Handle(ResolveForeclosurePreventionCaseCommand cmd, CancellationToken ct)
    {
        var fpc = await repository.GetByIdOrThrowAsync(ForeclosurePreventionCaseId.Create(cmd.ForeclosurePreventionCaseId), ct).ConfigureAwait(false);
        fpc.Resolve(cmd.Outcome, cmd.ResolvedDate, UserId.Create(cmd.ResolvedBy));
        repository.Stage(fpc);
        await unitOfWork.CommitAsync(ct).ConfigureAwait(false);
    }
}
