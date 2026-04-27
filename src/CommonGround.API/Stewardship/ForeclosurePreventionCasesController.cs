using CommonGround.API.Contracts.Stewardship.Requests;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Persistence;
using CommonGround.Stewardship.Application.Commands.ForeclosurePreventionCases;
using CommonGround.Stewardship.Application.Queries;
using CommonGround.Stewardship.Domain.ForeclosurePreventionCases;
using CommonGround.Stewardship.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CommonGround.API.Stewardship;

/// <summary>API controller for foreclosure prevention case management.</summary>
[ApiController]
[Route("api/stewardship/foreclosure-prevention")]
public sealed class ForeclosurePreventionCasesController(
    IMessageBus bus,
    IForeclosurePreventionCaseRepository repository,
    IUnitOfWork unitOfWork) : ControllerBase
{
    /// <summary>Gets a foreclosure prevention case by identifier.</summary>
    /// <param name="id">The foreclosure prevention case identifier.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The case when found; otherwise a 404 response.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ForeclosurePreventionCaseView), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(string id, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<ForeclosurePreventionCaseView?>(
            new GetForeclosurePreventionCaseQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Gets foreclosure prevention cases. Returns all non-resolved cases when groundLeaseId is omitted.</summary>
    /// <param name="groundLeaseId">Optional ground lease identifier filter.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A list of foreclosure prevention cases.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ForeclosurePreventionCaseView>), 200)]
    public async Task<IActionResult> GetByGroundLease([FromQuery] string? groundLeaseId, CancellationToken ct)
    {
        IReadOnlyList<ForeclosurePreventionCaseView> result = string.IsNullOrWhiteSpace(groundLeaseId)
            ? await bus.InvokeAsync<IReadOnlyList<ForeclosurePreventionCaseView>>(
                new GetAllForeclosurePreventionCasesQuery(), ct)
            : await bus.InvokeAsync<IReadOnlyList<ForeclosurePreventionCaseView>>(
                new GetForeclosurePreventionCasesByGroundLeaseQuery(groundLeaseId), ct);
        return Ok(result);
    }

    /// <summary>Opens a foreclosure prevention case from a servicer delinquency trigger.</summary>
    /// <param name="request">The payload used to open the case and record delinquency.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>An accepted response containing the new case identifier.</returns>
    [HttpPost]
    [ProducesResponseType(202)]
    public async Task<IActionResult> OpenForServicerDelinquency(
        [FromBody] OpenForeclosurePreventionCaseRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = HttpContext.GetUserId();

        var fpc = ForeclosurePreventionCase.Open(
            request.GroundLeaseId,
            ForeclosurePreventionTrigger.ServicerDelinquency,
            request.RecordedDate,
            userId);

        fpc.RecordServicerDelinquency(
            request.ServicerName,
            request.DelinquentMonths,
            request.RecordedDate,
            userId);

        repository.Stage(fpc);
        await unitOfWork.CommitAsync(ct);

        return Accepted(new { Id = fpc.Id.Value });
    }

    /// <summary>Records a servicer delinquency for an existing case.</summary>
    /// <param name="id">The foreclosure prevention case identifier.</param>
    /// <param name="request">The servicer delinquency payload.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A no-content response.</returns>
    [HttpPost("{id}/servicer-delinquency")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> RecordServicerDelinquency(
        string id,
        [FromBody] RecordServicerDelinquencyRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = HttpContext.GetUserId();
        await bus.InvokeAsync(new RecordServicerDelinquencyCommand(
            id, request.ServicerName, request.DelinquentMonths, request.RecordedDate, userId.Value), ct);
        return NoContent();
    }

    /// <summary>Creates a housing counseling referral for a case.</summary>
    /// <param name="id">The foreclosure prevention case identifier.</param>
    /// <param name="request">The counseling referral payload.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A no-content response.</returns>
    [HttpPost("{id}/counseling-referrals")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> MakeCounselingReferral(
        string id,
        [FromBody] MakeCounselingReferralRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = HttpContext.GetUserId();
        await bus.InvokeAsync(new MakeCounselingReferralCommand(
            id, request.AgencyName, request.ReferralDate, userId.Value), ct);
        return NoContent();
    }

    /// <summary>Records the outcome of a counseling referral.</summary>
    /// <param name="id">The foreclosure prevention case identifier.</param>
    /// <param name="referralId">The counseling referral identifier.</param>
    /// <param name="request">The counseling outcome payload.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A no-content response.</returns>
    [HttpPost("{id}/counseling-referrals/{referralId}/outcome")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> RecordCounselingOutcome(
        string id,
        Guid referralId,
        [FromBody] RecordCounselingOutcomeRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = HttpContext.GetUserId();
        await bus.InvokeAsync(new RecordCounselingOutcomeCommand(
            id, referralId, request.Outcome, request.OutcomeDate, userId.Value), ct);
        return NoContent();
    }

    /// <summary>Submits a forbearance request for a case.</summary>
    /// <param name="id">The foreclosure prevention case identifier.</param>
    /// <param name="request">The forbearance request payload.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A no-content response.</returns>
    [HttpPost("{id}/forbearance-requests")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> RequestForbearance(
        string id,
        [FromBody] RequestForbearanceRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = HttpContext.GetUserId();
        await bus.InvokeAsync(new RequestForbearanceCommand(
            id, request.LenderName, request.RequestedDate, userId.Value), ct);
        return NoContent();
    }

    /// <summary>Records the outcome of a forbearance request.</summary>
    /// <param name="id">The foreclosure prevention case identifier.</param>
    /// <param name="requestId">The forbearance request identifier.</param>
    /// <param name="request">The forbearance outcome payload.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A no-content response.</returns>
    [HttpPost("{id}/forbearance-requests/{requestId}/outcome")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> RecordForbearanceOutcome(
        string id,
        Guid requestId,
        [FromBody] RecordForbearanceOutcomeRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = HttpContext.GetUserId();
        var outcome = Enum.Parse<ForbearanceOutcome>(request.Outcome);
        await bus.InvokeAsync(new RecordForbearanceOutcomeCommand(
            id, requestId, outcome, request.OutcomeDate, request.Notes, userId.Value), ct);
        return NoContent();
    }

    /// <summary>Activates the cure period for a foreclosure prevention case.</summary>
    /// <param name="id">The foreclosure prevention case identifier.</param>
    /// <param name="request">The cure period activation payload.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A no-content response.</returns>
    [HttpPost("{id}/cure-period")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> ActivateCurePeriod(
        string id,
        [FromBody] ActivateCurePeriodRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = HttpContext.GetUserId();
        await bus.InvokeAsync(new ActivateCurePeriodCommand(
            id, request.LraId, request.ActivationDate, request.CurePeriodDays, userId.Value), ct);
        return NoContent();
    }

    /// <summary>Resolves a foreclosure prevention case.</summary>
    /// <param name="id">The foreclosure prevention case identifier.</param>
    /// <param name="request">The case resolution payload.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A no-content response.</returns>
    [HttpPost("{id}/resolve")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> ResolveCase(
        string id,
        [FromBody] ResolveForeclosurePreventionCaseRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = HttpContext.GetUserId();
        var outcome = Enum.Parse<ForeclosurePreventionOutcome>(request.Outcome);
        await bus.InvokeAsync(new ResolveForeclosurePreventionCaseCommand(
            id, outcome, request.ResolvedDate, userId.Value), ct);
        return NoContent();
    }
}
