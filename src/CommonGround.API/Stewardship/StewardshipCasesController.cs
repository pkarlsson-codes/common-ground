using CommonGround.API.Contracts.Stewardship.Requests;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.Stewardship.Application.Commands.StewardshipCases;
using CommonGround.Stewardship.Application.Queries;
using CommonGround.Stewardship.Domain.StewardshipCases;
using CommonGround.Stewardship.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CommonGround.API.Stewardship;

/// <summary>API controller for stewardship case management.</summary>
[ApiController]
[Route("api/stewardship/cases")]
public sealed class StewardshipCasesController(IMessageBus bus) : ControllerBase
{
    /// <summary>Gets a stewardship case by identifier.</summary>
    /// <param name="id">The stewardship case identifier.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The stewardship case when found; otherwise a 404 response.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(StewardshipCaseView), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(string id, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<StewardshipCaseView?>(new GetStewardshipCaseQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Gets stewardship cases. Returns all active cases when groundLeaseId is omitted.</summary>
    /// <param name="groundLeaseId">Optional ground lease identifier filter.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A list of stewardship cases.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<StewardshipCaseView>), 200)]
    public async Task<IActionResult> GetByGroundLease([FromQuery] string? groundLeaseId, CancellationToken ct)
    {
        IReadOnlyList<StewardshipCaseView> result = string.IsNullOrWhiteSpace(groundLeaseId)
            ? await bus.InvokeAsync<IReadOnlyList<StewardshipCaseView>>(new GetAllStewardshipCasesQuery(), ct)
            : await bus.InvokeAsync<IReadOnlyList<StewardshipCaseView>>(
                new GetStewardshipCasesByGroundLeaseQuery(groundLeaseId), ct);
        return Ok(result);
    }

    /// <summary>Initiates an occupancy verification period for a stewardship case.</summary>
    /// <param name="id">The stewardship case identifier.</param>
    /// <param name="request">The occupancy verification initiation payload.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A no-content response.</returns>
    [HttpPost("{id}/occupancy-verifications")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> InitiateOccupancyVerification(
        string id,
        [FromBody] InitiateOccupancyVerificationRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = HttpContext.GetUserId();
        await bus.InvokeAsync(new InitiateOccupancyVerificationCommand(
            id, request.PeriodYear, request.SurveyDeadline, userId.Value), ct);
        return NoContent();
    }

    /// <summary>Submits a completed occupancy verification.</summary>
    /// <param name="id">The stewardship case identifier.</param>
    /// <param name="verificationId">The occupancy verification identifier.</param>
    /// <param name="request">The occupancy verification submission payload.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A no-content response.</returns>
    [HttpPost("{id}/occupancy-verifications/{verificationId}/submit")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> SubmitOccupancyVerification(
        string id,
        Guid verificationId,
        [FromBody] SubmitOccupancyVerificationRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        await bus.InvokeAsync(new SubmitOccupancyVerificationCommand(
            id, verificationId, request.IsPrimaryResident, request.SubletDisclosed,
            request.MajorChanges, request.SubmittedDate), ct);
        return NoContent();
    }

    /// <summary>Marks an occupancy verification as verified.</summary>
    /// <param name="id">The stewardship case identifier.</param>
    /// <param name="verificationId">The occupancy verification identifier.</param>
    /// <param name="request">The occupancy verification verification payload.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A no-content response.</returns>
    [HttpPost("{id}/occupancy-verifications/{verificationId}/verify")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> VerifyOccupancy(
        string id,
        Guid verificationId,
        [FromBody] VerifyOccupancyRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = HttpContext.GetUserId();
        await bus.InvokeAsync(new VerifyOccupancyCommand(
            id, verificationId, request.VerifiedDate, userId.Value), ct);
        return NoContent();
    }

    /// <summary>Marks an occupancy verification as failed.</summary>
    /// <param name="id">The stewardship case identifier.</param>
    /// <param name="verificationId">The occupancy verification identifier.</param>
    /// <param name="request">The occupancy verification failure payload.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A no-content response.</returns>
    [HttpPost("{id}/occupancy-verifications/{verificationId}/fail")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> FailOccupancy(
        string id,
        Guid verificationId,
        [FromBody] FailOccupancyRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = HttpContext.GetUserId();
        await bus.InvokeAsync(new FailOccupancyCommand(
            id, verificationId, request.Reason, request.FailedDate, userId.Value), ct);
        return NoContent();
    }

    /// <summary>Marks an occupancy verification as overdue.</summary>
    /// <param name="id">The stewardship case identifier.</param>
    /// <param name="verificationId">The occupancy verification identifier.</param>
    /// <param name="request">The occupancy verification overdue payload.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A no-content response.</returns>
    [HttpPost("{id}/occupancy-verifications/{verificationId}/overdue")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> MarkOccupancyVerificationOverdue(
        string id,
        Guid verificationId,
        [FromBody] MarkOccupancyVerificationOverdueRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        await bus.InvokeAsync(new MarkOccupancyVerificationOverdueCommand(
            id, verificationId, request.Deadline), ct);
        return NoContent();
    }

    /// <summary>Schedules a property inspection.</summary>
    /// <param name="id">The stewardship case identifier.</param>
    /// <param name="request">The inspection scheduling payload.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A no-content response.</returns>
    [HttpPost("{id}/inspections")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> SchedulePropertyInspection(
        string id,
        [FromBody] SchedulePropertyInspectionRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = HttpContext.GetUserId();
        await bus.InvokeAsync(new SchedulePropertyInspectionCommand(
            id, request.ScheduledDate, request.InspectorName, userId.Value), ct);
        return NoContent();
    }

    /// <summary>Completes a scheduled property inspection.</summary>
    /// <param name="id">The stewardship case identifier.</param>
    /// <param name="inspectionId">The property inspection identifier.</param>
    /// <param name="request">The inspection completion payload.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A no-content response.</returns>
    [HttpPost("{id}/inspections/{inspectionId}/complete")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> CompletePropertyInspection(
        string id,
        Guid inspectionId,
        [FromBody] CompletePropertyInspectionRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = HttpContext.GetUserId();
        await bus.InvokeAsync(new CompletePropertyInspectionCommand(
            id, inspectionId, request.OccupancyConfirmed, request.ConditionNotes,
            request.CompletedDate, userId.Value), ct);
        return NoContent();
    }

    /// <summary>Cancels a scheduled property inspection.</summary>
    /// <param name="id">The stewardship case identifier.</param>
    /// <param name="inspectionId">The property inspection identifier.</param>
    /// <param name="request">The inspection cancellation payload.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A no-content response.</returns>
    [HttpPost("{id}/inspections/{inspectionId}/cancel")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> CancelPropertyInspection(
        string id,
        Guid inspectionId,
        [FromBody] CancelPropertyInspectionRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = HttpContext.GetUserId();
        await bus.InvokeAsync(new CancelPropertyInspectionCommand(
            id, inspectionId, request.Reason, userId.Value), ct);
        return NoContent();
    }

    /// <summary>Identifies a lease violation for a stewardship case.</summary>
    /// <param name="id">The stewardship case identifier.</param>
    /// <param name="request">The violation identification payload.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A no-content response.</returns>
    [HttpPost("{id}/violations")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> IdentifyViolation(
        string id,
        [FromBody] IdentifyViolationRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = HttpContext.GetUserId();
        var violationType = Enum.Parse<LeaseViolationType>(request.ViolationType);
        await bus.InvokeAsync(new IdentifyViolationCommand(
            id, violationType, request.Description, request.IdentifiedDate, userId.Value), ct);
        return NoContent();
    }

    /// <summary>Issues a notice for a lease violation.</summary>
    /// <param name="id">The stewardship case identifier.</param>
    /// <param name="violationId">The lease violation identifier.</param>
    /// <param name="request">The violation notice payload.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A no-content response.</returns>
    [HttpPost("{id}/violations/{violationId}/notice")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> IssueViolationNotice(
        string id,
        Guid violationId,
        [FromBody] IssueViolationNoticeRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = HttpContext.GetUserId();
        await bus.InvokeAsync(new IssueViolationNoticeCommand(
            id, violationId, request.NoticeDate, request.CureDeadline, userId.Value), ct);
        return NoContent();
    }

    /// <summary>Records that a lease violation has been cured.</summary>
    /// <param name="id">The stewardship case identifier.</param>
    /// <param name="violationId">The lease violation identifier.</param>
    /// <param name="request">The violation cure payload.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A no-content response.</returns>
    [HttpPost("{id}/violations/{violationId}/cure")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> CureViolation(
        string id,
        Guid violationId,
        [FromBody] CureViolationRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = HttpContext.GetUserId();
        await bus.InvokeAsync(new CureViolationCommand(
            id, violationId, request.CuredDate, userId.Value), ct);
        return NoContent();
    }

    /// <summary>Escalates a lease violation.</summary>
    /// <param name="id">The stewardship case identifier.</param>
    /// <param name="violationId">The lease violation identifier.</param>
    /// <param name="request">The violation escalation payload.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A no-content response.</returns>
    [HttpPost("{id}/violations/{violationId}/escalate")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> EscalateViolation(
        string id,
        Guid violationId,
        [FromBody] EscalateViolationRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = HttpContext.GetUserId();
        await bus.InvokeAsync(new EscalateViolationCommand(
            id, violationId, request.Reason, request.EscalatedDate, userId.Value), ct);
        return NoContent();
    }

    /// <summary>Closes an open stewardship case.</summary>
    /// <param name="id">The stewardship case identifier.</param>
    /// <param name="request">The case closure payload.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A no-content response.</returns>
    [HttpPost("{id}/close")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> CloseStewardshipCase(
        string id,
        [FromBody] CloseStewardshipCaseRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = HttpContext.GetUserId();
        await bus.InvokeAsync(new CloseStewardshipCaseCommand(
            id, request.Reason, request.ClosedDate, userId.Value), ct);
        return NoContent();
    }
}
