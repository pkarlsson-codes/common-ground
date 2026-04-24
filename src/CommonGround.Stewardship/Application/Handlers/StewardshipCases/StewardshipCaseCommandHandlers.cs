using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Persistence;
using CommonGround.Stewardship.Application.Commands.StewardshipCases;
using CommonGround.Stewardship.Domain.StewardshipCases;

namespace CommonGround.Stewardship.Application.Handlers.StewardshipCases;

/// <summary>
/// Handles commands for stewardship cases.
/// </summary>
public sealed class StewardshipCaseCommandHandler(
    IStewardshipCaseRepository repository,
    IUnitOfWork unitOfWork)
{
    /// <summary>
    /// Handles the InitiateOccupancyVerificationCommand.
    /// </summary>
    public async Task Handle(InitiateOccupancyVerificationCommand cmd, CancellationToken ct)
    {
        var sc = await repository.GetByIdOrThrowAsync(StewardshipCaseId.Create(cmd.StewardshipCaseId), ct).ConfigureAwait(false);
        sc.InitiateOccupancyVerification(cmd.PeriodYear, cmd.SurveyDeadline, UserId.Create(cmd.InitiatedBy));
        repository.Stage(sc);
        await unitOfWork.CommitAsync(ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the SubmitOccupancyVerificationCommand.
    /// </summary>
    public async Task Handle(SubmitOccupancyVerificationCommand cmd, CancellationToken ct)
    {
        var sc = await repository.GetByIdOrThrowAsync(StewardshipCaseId.Create(cmd.StewardshipCaseId), ct).ConfigureAwait(false);
        sc.SubmitOccupancyVerification(cmd.VerificationId, cmd.IsPrimaryResident, cmd.SubletDisclosed, cmd.MajorChanges, cmd.SubmittedDate);
        repository.Stage(sc);
        await unitOfWork.CommitAsync(ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the VerifyOccupancyCommand.
    /// </summary>
    public async Task Handle(VerifyOccupancyCommand cmd, CancellationToken ct)
    {
        var sc = await repository.GetByIdOrThrowAsync(StewardshipCaseId.Create(cmd.StewardshipCaseId), ct).ConfigureAwait(false);
        sc.VerifyOccupancy(cmd.VerificationId, cmd.VerifiedDate, UserId.Create(cmd.VerifiedBy));
        repository.Stage(sc);
        await unitOfWork.CommitAsync(ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the FailOccupancyCommand.
    /// </summary>
    public async Task Handle(FailOccupancyCommand cmd, CancellationToken ct)
    {
        var sc = await repository.GetByIdOrThrowAsync(StewardshipCaseId.Create(cmd.StewardshipCaseId), ct).ConfigureAwait(false);
        sc.FailOccupancy(cmd.VerificationId, cmd.Reason, cmd.FailedDate, UserId.Create(cmd.RecordedBy));
        repository.Stage(sc);
        await unitOfWork.CommitAsync(ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the MarkOccupancyVerificationOverdueCommand.
    /// </summary>
    public async Task Handle(MarkOccupancyVerificationOverdueCommand cmd, CancellationToken ct)
    {
        var sc = await repository.GetByIdOrThrowAsync(StewardshipCaseId.Create(cmd.StewardshipCaseId), ct).ConfigureAwait(false);
        sc.MarkOccupancyVerificationOverdue(cmd.VerificationId, cmd.Deadline);
        repository.Stage(sc);
        await unitOfWork.CommitAsync(ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the SchedulePropertyInspectionCommand.
    /// </summary>
    public async Task Handle(SchedulePropertyInspectionCommand cmd, CancellationToken ct)
    {
        var sc = await repository.GetByIdOrThrowAsync(StewardshipCaseId.Create(cmd.StewardshipCaseId), ct).ConfigureAwait(false);
        sc.SchedulePropertyInspection(cmd.ScheduledDate, cmd.InspectorName, UserId.Create(cmd.ScheduledBy));
        repository.Stage(sc);
        await unitOfWork.CommitAsync(ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the CompletePropertyInspectionCommand.
    /// </summary>
    public async Task Handle(CompletePropertyInspectionCommand cmd, CancellationToken ct)
    {
        var sc = await repository.GetByIdOrThrowAsync(StewardshipCaseId.Create(cmd.StewardshipCaseId), ct).ConfigureAwait(false);
        sc.CompletePropertyInspection(cmd.InspectionId, cmd.OccupancyConfirmed, cmd.ConditionNotes, cmd.CompletedDate, UserId.Create(cmd.RecordedBy));
        repository.Stage(sc);
        await unitOfWork.CommitAsync(ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the CancelPropertyInspectionCommand.
    /// </summary>
    public async Task Handle(CancelPropertyInspectionCommand cmd, CancellationToken ct)
    {
        var sc = await repository.GetByIdOrThrowAsync(StewardshipCaseId.Create(cmd.StewardshipCaseId), ct).ConfigureAwait(false);
        sc.CancelPropertyInspection(cmd.InspectionId, cmd.Reason, UserId.Create(cmd.CancelledBy));
        repository.Stage(sc);
        await unitOfWork.CommitAsync(ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the IdentifyViolationCommand.
    /// </summary>
    public async Task Handle(IdentifyViolationCommand cmd, CancellationToken ct)
    {
        var sc = await repository.GetByIdOrThrowAsync(StewardshipCaseId.Create(cmd.StewardshipCaseId), ct).ConfigureAwait(false);
        sc.IdentifyViolation(cmd.Type, cmd.Description, cmd.IdentifiedDate, UserId.Create(cmd.IdentifiedBy));
        repository.Stage(sc);
        await unitOfWork.CommitAsync(ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the IssueViolationNoticeCommand.
    /// </summary>
    public async Task Handle(IssueViolationNoticeCommand cmd, CancellationToken ct)
    {
        var sc = await repository.GetByIdOrThrowAsync(StewardshipCaseId.Create(cmd.StewardshipCaseId), ct).ConfigureAwait(false);
        sc.IssueViolationNotice(cmd.ViolationId, cmd.NoticeDate, cmd.CureDeadline, UserId.Create(cmd.IssuedBy));
        repository.Stage(sc);
        await unitOfWork.CommitAsync(ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the CureViolationCommand.
    /// </summary>
    public async Task Handle(CureViolationCommand cmd, CancellationToken ct)
    {
        var sc = await repository.GetByIdOrThrowAsync(StewardshipCaseId.Create(cmd.StewardshipCaseId), ct).ConfigureAwait(false);
        sc.CureViolation(cmd.ViolationId, cmd.CuredDate, UserId.Create(cmd.VerifiedBy));
        repository.Stage(sc);
        await unitOfWork.CommitAsync(ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the EscalateViolationCommand.
    /// </summary>
    public async Task Handle(EscalateViolationCommand cmd, CancellationToken ct)
    {
        var sc = await repository.GetByIdOrThrowAsync(StewardshipCaseId.Create(cmd.StewardshipCaseId), ct).ConfigureAwait(false);
        sc.EscalateViolation(cmd.ViolationId, cmd.Reason, cmd.EscalatedDate, UserId.Create(cmd.EscalatedBy));
        repository.Stage(sc);
        await unitOfWork.CommitAsync(ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the CloseStewardshipCaseCommand.
    /// </summary>
    public async Task Handle(CloseStewardshipCaseCommand cmd, CancellationToken ct)
    {
        var sc = await repository.GetByIdOrThrowAsync(StewardshipCaseId.Create(cmd.StewardshipCaseId), ct).ConfigureAwait(false);
        sc.Close(cmd.Reason, cmd.ClosedDate, UserId.Create(cmd.ClosedBy));
        repository.Stage(sc);
        await unitOfWork.CommitAsync(ct).ConfigureAwait(false);
    }
}
