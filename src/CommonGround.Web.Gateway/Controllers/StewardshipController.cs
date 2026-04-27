using CommonGround.API.Clients;
using CommonGround.API.Contracts.Stewardship.Requests;
using CommonGround.API.Contracts.Stewardship.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CommonGround.Web.Gateway.Controllers;

/// <summary>Gateway proxy for stewardship endpoints.</summary>
/// <param name="client">Typed client for the Stewardship API.</param>
[ApiController]
[Route("api/v1/stewardship")]
public sealed class StewardshipController(IStewardshipApiClient client) : ControllerBase
{
    // -----------------------------------------------------------------------
    // Stewardship Cases
    // -----------------------------------------------------------------------

    /// <summary>Lists all active stewardship cases.</summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>200 with a list of active stewardship cases.</returns>
    [HttpGet("cases")]
    [ProducesResponseType(typeof(IReadOnlyList<StewardshipCaseResponse>), 200)]
    [EndpointName("getAllStewardshipCases")]
    public async Task<IActionResult> GetAllStewardshipCases(CancellationToken cancellationToken)
    {
        var result = await client.GetAllStewardshipCasesAsync(cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>Gets stewardship cases for a specific ground lease.</summary>
    /// <param name="groundLeaseId">The ground lease identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>200 with the list of stewardship cases.</returns>
    [HttpGet("cases/by-ground-lease/{groundLeaseId}")]
    [ProducesResponseType(typeof(IReadOnlyList<StewardshipCaseResponse>), 200)]
    [EndpointName("getStewardshipCasesByGroundLease")]
    public async Task<IActionResult> GetStewardshipCasesByGroundLease(
        string groundLeaseId,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        var result = await client.GetStewardshipCasesByGroundLeaseAsync(groundLeaseId, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>Gets a stewardship case by ID.</summary>
    /// <param name="caseId">The stewardship case identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>200 with the stewardship case, or 404 if not found.</returns>
    [HttpGet("cases/{caseId}")]
    [ProducesResponseType(typeof(StewardshipCaseResponse), 200)]
    [ProducesResponseType(404)]
    [EndpointName("getStewardshipCase")]
    public async Task<IActionResult> GetStewardshipCase(string caseId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        var result = await client.GetStewardshipCaseAsync(caseId, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>Initiates an annual occupancy verification.</summary>
    /// <param name="caseId">The stewardship case identifier.</param>
    /// <param name="request">The verification initiation details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content.</returns>
    [HttpPost("cases/{caseId}/occupancy-verifications")]
    [ProducesResponseType(204)]
    [EndpointName("initiateOccupancyVerification")]
    public async Task<IActionResult> InitiateOccupancyVerification(
        string caseId,
        [FromBody] InitiateOccupancyVerificationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.InitiateOccupancyVerificationAsync(caseId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Submits homeowner responses to an occupancy verification.</summary>
    /// <param name="caseId">The stewardship case identifier.</param>
    /// <param name="verificationId">The occupancy verification identifier.</param>
    /// <param name="request">The submission details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content.</returns>
    [HttpPost("cases/{caseId}/occupancy-verifications/{verificationId}/submit")]
    [ProducesResponseType(204)]
    [EndpointName("submitOccupancyVerification")]
    public async Task<IActionResult> SubmitOccupancyVerification(
        string caseId,
        Guid verificationId,
        [FromBody] SubmitOccupancyVerificationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.SubmitOccupancyVerificationAsync(caseId, verificationId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Records staff approval of an occupancy verification.</summary>
    /// <param name="caseId">The stewardship case identifier.</param>
    /// <param name="verificationId">The occupancy verification identifier.</param>
    /// <param name="request">The verification approval details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content.</returns>
    [HttpPost("cases/{caseId}/occupancy-verifications/{verificationId}/verify")]
    [ProducesResponseType(204)]
    [EndpointName("verifyOccupancy")]
    public async Task<IActionResult> VerifyOccupancy(
        string caseId,
        Guid verificationId,
        [FromBody] VerifyOccupancyRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.VerifyOccupancyAsync(caseId, verificationId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Records a failed occupancy verification.</summary>
    /// <param name="caseId">The stewardship case identifier.</param>
    /// <param name="verificationId">The occupancy verification identifier.</param>
    /// <param name="request">The failure details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content.</returns>
    [HttpPost("cases/{caseId}/occupancy-verifications/{verificationId}/fail")]
    [ProducesResponseType(204)]
    [EndpointName("failOccupancy")]
    public async Task<IActionResult> FailOccupancy(
        string caseId,
        Guid verificationId,
        [FromBody] FailOccupancyRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.FailOccupancyAsync(caseId, verificationId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Marks an occupancy verification as overdue.</summary>
    /// <param name="caseId">The stewardship case identifier.</param>
    /// <param name="verificationId">The occupancy verification identifier.</param>
    /// <param name="request">The overdue details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content.</returns>
    [HttpPost("cases/{caseId}/occupancy-verifications/{verificationId}/overdue")]
    [ProducesResponseType(204)]
    [EndpointName("markOccupancyVerificationOverdue")]
    public async Task<IActionResult> MarkOccupancyVerificationOverdue(
        string caseId,
        Guid verificationId,
        [FromBody] MarkOccupancyVerificationOverdueRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.MarkOccupancyVerificationOverdueAsync(caseId, verificationId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Schedules a property inspection.</summary>
    /// <param name="caseId">The stewardship case identifier.</param>
    /// <param name="request">The inspection scheduling details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content.</returns>
    [HttpPost("cases/{caseId}/inspections")]
    [ProducesResponseType(204)]
    [EndpointName("schedulePropertyInspection")]
    public async Task<IActionResult> SchedulePropertyInspection(
        string caseId,
        [FromBody] SchedulePropertyInspectionRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.SchedulePropertyInspectionAsync(caseId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Records the completion of a property inspection.</summary>
    /// <param name="caseId">The stewardship case identifier.</param>
    /// <param name="inspectionId">The property inspection identifier.</param>
    /// <param name="request">The inspection completion details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content.</returns>
    [HttpPost("cases/{caseId}/inspections/{inspectionId}/complete")]
    [ProducesResponseType(204)]
    [EndpointName("completePropertyInspection")]
    public async Task<IActionResult> CompletePropertyInspection(
        string caseId,
        Guid inspectionId,
        [FromBody] CompletePropertyInspectionRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.CompletePropertyInspectionAsync(caseId, inspectionId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Cancels a scheduled property inspection.</summary>
    /// <param name="caseId">The stewardship case identifier.</param>
    /// <param name="inspectionId">The property inspection identifier.</param>
    /// <param name="request">The cancellation details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content.</returns>
    [HttpPost("cases/{caseId}/inspections/{inspectionId}/cancel")]
    [ProducesResponseType(204)]
    [EndpointName("cancelPropertyInspection")]
    public async Task<IActionResult> CancelPropertyInspection(
        string caseId,
        Guid inspectionId,
        [FromBody] CancelPropertyInspectionRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.CancelPropertyInspectionAsync(caseId, inspectionId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Identifies a new lease violation.</summary>
    /// <param name="caseId">The stewardship case identifier.</param>
    /// <param name="request">The violation identification details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content.</returns>
    [HttpPost("cases/{caseId}/violations")]
    [ProducesResponseType(204)]
    [EndpointName("identifyViolation")]
    public async Task<IActionResult> IdentifyViolation(
        string caseId,
        [FromBody] IdentifyViolationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.IdentifyViolationAsync(caseId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Issues a formal notice for a lease violation.</summary>
    /// <param name="caseId">The stewardship case identifier.</param>
    /// <param name="violationId">The lease violation identifier.</param>
    /// <param name="request">The notice details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content.</returns>
    [HttpPost("cases/{caseId}/violations/{violationId}/notice")]
    [ProducesResponseType(204)]
    [EndpointName("issueViolationNotice")]
    public async Task<IActionResult> IssueViolationNotice(
        string caseId,
        Guid violationId,
        [FromBody] IssueViolationNoticeRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.IssueViolationNoticeAsync(caseId, violationId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Records that a lease violation has been cured.</summary>
    /// <param name="caseId">The stewardship case identifier.</param>
    /// <param name="violationId">The lease violation identifier.</param>
    /// <param name="request">The cure details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content.</returns>
    [HttpPost("cases/{caseId}/violations/{violationId}/cure")]
    [ProducesResponseType(204)]
    [EndpointName("cureViolation")]
    public async Task<IActionResult> CureViolation(
        string caseId,
        Guid violationId,
        [FromBody] CureViolationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.CureViolationAsync(caseId, violationId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Escalates an unresolved lease violation.</summary>
    /// <param name="caseId">The stewardship case identifier.</param>
    /// <param name="violationId">The lease violation identifier.</param>
    /// <param name="request">The escalation details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content.</returns>
    [HttpPost("cases/{caseId}/violations/{violationId}/escalate")]
    [ProducesResponseType(204)]
    [EndpointName("escalateViolation")]
    public async Task<IActionResult> EscalateViolation(
        string caseId,
        Guid violationId,
        [FromBody] EscalateViolationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.EscalateViolationAsync(caseId, violationId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Closes a stewardship case.</summary>
    /// <param name="caseId">The stewardship case identifier.</param>
    /// <param name="request">The closure details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content.</returns>
    [HttpPost("cases/{caseId}/close")]
    [ProducesResponseType(204)]
    [EndpointName("closeStewardshipCase")]
    public async Task<IActionResult> CloseStewardshipCase(
        string caseId,
        [FromBody] CloseStewardshipCaseRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.CloseStewardshipCaseAsync(caseId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    // -----------------------------------------------------------------------
    // Foreclosure Prevention Cases
    // -----------------------------------------------------------------------

    /// <summary>Lists all non-resolved foreclosure prevention cases.</summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>200 with a list of open/active foreclosure prevention cases.</returns>
    [HttpGet("foreclosure-prevention")]
    [ProducesResponseType(typeof(IReadOnlyList<ForeclosurePreventionCaseResponse>), 200)]
    [EndpointName("getAllForeclosurePreventionCases")]
    public async Task<IActionResult> GetAllForeclosurePreventionCases(CancellationToken cancellationToken)
    {
        var result = await client.GetAllForeclosurePreventionCasesAsync(cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>Gets foreclosure prevention cases for a specific ground lease.</summary>
    /// <param name="groundLeaseId">The ground lease identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>200 with the list of foreclosure prevention cases.</returns>
    [HttpGet("foreclosure-prevention/by-ground-lease/{groundLeaseId}")]
    [ProducesResponseType(typeof(IReadOnlyList<ForeclosurePreventionCaseResponse>), 200)]
    [EndpointName("getForeclosurePreventionCasesByGroundLease")]
    public async Task<IActionResult> GetForeclosurePreventionCasesByGroundLease(
        string groundLeaseId,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        var result = await client.GetForeclosurePreventionCasesByGroundLeaseAsync(groundLeaseId, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>Opens a new foreclosure prevention case.</summary>
    /// <param name="request">The case opening details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>202 Accepted with the new case ID.</returns>
    [HttpPost("foreclosure-prevention")]
    [ProducesResponseType(typeof(OpenForeclosurePreventionCaseResponse), 202)]
    [EndpointName("openForeclosurePreventionCase")]
    public async Task<IActionResult> OpenForeclosurePreventionCase(
        [FromBody] OpenForeclosurePreventionCaseRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var result = await client.OpenForeclosurePreventionCaseAsync(request, cancellationToken).ConfigureAwait(false);
        return AcceptedAtAction(nameof(GetForeclosurePreventionCase), new { caseId = result.Id }, result);
    }

    /// <summary>Gets a foreclosure prevention case by ID.</summary>
    /// <param name="caseId">The foreclosure prevention case identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>200 with the case, or 404 if not found.</returns>
    [HttpGet("foreclosure-prevention/{caseId}")]
    [ProducesResponseType(typeof(ForeclosurePreventionCaseResponse), 200)]
    [ProducesResponseType(404)]
    [EndpointName("getForeclosurePreventionCase")]
    public async Task<IActionResult> GetForeclosurePreventionCase(string caseId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        var result = await client.GetForeclosurePreventionCaseAsync(caseId, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>Records a servicer delinquency.</summary>
    /// <param name="caseId">The foreclosure prevention case identifier.</param>
    /// <param name="request">The delinquency details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content.</returns>
    [HttpPost("foreclosure-prevention/{caseId}/servicer-delinquency")]
    [ProducesResponseType(204)]
    [EndpointName("recordServicerDelinquency")]
    public async Task<IActionResult> RecordServicerDelinquency(
        string caseId,
        [FromBody] RecordServicerDelinquencyRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.RecordServicerDelinquencyAsync(caseId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Makes a housing counseling referral.</summary>
    /// <param name="caseId">The foreclosure prevention case identifier.</param>
    /// <param name="request">The referral details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content.</returns>
    [HttpPost("foreclosure-prevention/{caseId}/counseling-referrals")]
    [ProducesResponseType(204)]
    [EndpointName("makeCounselingReferral")]
    public async Task<IActionResult> MakeCounselingReferral(
        string caseId,
        [FromBody] MakeCounselingReferralRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.MakeCounselingReferralAsync(caseId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Records the outcome of a counseling referral.</summary>
    /// <param name="caseId">The foreclosure prevention case identifier.</param>
    /// <param name="referralId">The counseling referral identifier.</param>
    /// <param name="request">The outcome details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content.</returns>
    [HttpPost("foreclosure-prevention/{caseId}/counseling-referrals/{referralId}/outcome")]
    [ProducesResponseType(204)]
    [EndpointName("recordCounselingOutcome")]
    public async Task<IActionResult> RecordCounselingOutcome(
        string caseId,
        Guid referralId,
        [FromBody] RecordCounselingOutcomeRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.RecordCounselingOutcomeAsync(caseId, referralId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Submits a mortgage forbearance request.</summary>
    /// <param name="caseId">The foreclosure prevention case identifier.</param>
    /// <param name="request">The forbearance request details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content.</returns>
    [HttpPost("foreclosure-prevention/{caseId}/forbearance-requests")]
    [ProducesResponseType(204)]
    [EndpointName("requestForbearance")]
    public async Task<IActionResult> RequestForbearance(
        string caseId,
        [FromBody] RequestForbearanceRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.RequestForbearanceAsync(caseId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Records the outcome of a forbearance request.</summary>
    /// <param name="caseId">The foreclosure prevention case identifier.</param>
    /// <param name="requestId">The forbearance request identifier.</param>
    /// <param name="request">The outcome details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content.</returns>
    [HttpPost("foreclosure-prevention/{caseId}/forbearance-requests/{requestId}/outcome")]
    [ProducesResponseType(204)]
    [EndpointName("recordForbearanceOutcome")]
    public async Task<IActionResult> RecordForbearanceOutcome(
        string caseId,
        Guid requestId,
        [FromBody] RecordForbearanceOutcomeRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.RecordForbearanceOutcomeAsync(caseId, requestId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Activates an LRA cure period.</summary>
    /// <param name="caseId">The foreclosure prevention case identifier.</param>
    /// <param name="request">The cure period activation details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content.</returns>
    [HttpPost("foreclosure-prevention/{caseId}/cure-period")]
    [ProducesResponseType(204)]
    [EndpointName("activateCurePeriod")]
    public async Task<IActionResult> ActivateCurePeriod(
        string caseId,
        [FromBody] ActivateCurePeriodRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.ActivateCurePeriodAsync(caseId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Resolves a foreclosure prevention case.</summary>
    /// <param name="caseId">The foreclosure prevention case identifier.</param>
    /// <param name="request">The resolution details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content.</returns>
    [HttpPost("foreclosure-prevention/{caseId}/resolve")]
    [ProducesResponseType(204)]
    [EndpointName("resolveForeclosurePreventionCase")]
    public async Task<IActionResult> ResolveForeclosurePreventionCase(
        string caseId,
        [FromBody] ResolveForeclosurePreventionCaseRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.ResolveForeclosurePreventionCaseAsync(caseId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }
}
