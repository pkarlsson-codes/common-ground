using CommonGround.API.Clients;
using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.AspNetCore.Mvc;

namespace CommonGround.Web.Gateway.Controllers;

/// <summary>Gateway proxy for purchase workflow endpoints.</summary>
/// <param name="client">Typed client for the Homeownership API.</param>
[ApiController]
[Route("api/v1/purchase-workflows")]
public sealed class PurchaseWorkflowsController(IPurchaseWorkflowsApiClient client) : ControllerBase
{
    /// <summary>Initiates a new purchase workflow.</summary>
    /// <param name="request">The purchase workflow initiation details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>201 Created with the new purchase workflow ID.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(InitiatePurchaseWorkflowResponse), 201)]
    [EndpointName("initiatePurchaseWorkflow")]
    public async Task<IActionResult> InitiatePurchaseWorkflow(
        [FromBody] InitiatePurchaseWorkflowRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var result = await client.InitiatePurchaseWorkflowAsync(request, cancellationToken).ConfigureAwait(false);
        return CreatedAtAction(nameof(GetPurchaseWorkflow), new { workflowId = result.PurchaseWorkflowId }, result);
    }

    /// <summary>Gets a purchase workflow by ID.</summary>
    /// <param name="workflowId">The ID of the purchase workflow to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>200 with the purchase workflow, or 404 if not found.</returns>
    [HttpGet("{workflowId}")]
    [ProducesResponseType(typeof(PurchaseWorkflowResponse), 200)]
    [ProducesResponseType(404)]
    [EndpointName("getPurchaseWorkflow")]
    public async Task<IActionResult> GetPurchaseWorkflow(string workflowId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        var result = await client.GetPurchaseWorkflowAsync(workflowId, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>Gets a paginated list of purchase workflows.</summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="groundLeaseId">Optional ground lease ID filter.</param>
    /// <param name="phase">Optional phase filter.</param>
    /// <param name="search">Optional free-text search across buyer name.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>200 with a paginated list of purchase workflows.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<PurchaseWorkflowListItemResponse>), 200)]
    [EndpointName("getPurchaseWorkflows")]
    public async Task<IActionResult> GetPurchaseWorkflows(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? groundLeaseId = null,
        [FromQuery] string? phase = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await client.GetPurchaseWorkflowsAsync(page, pageSize, groundLeaseId, phase, search, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>Records buyer eligibility verified.</summary>
    /// <param name="workflowId">The ID of the purchase workflow.</param>
    /// <param name="request">The eligibility verification details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content.</returns>
    [HttpPost("{workflowId}/eligibility")]
    [ProducesResponseType(204)]
    [EndpointName("recordEligibilityVerified")]
    public async Task<IActionResult> RecordEligibilityVerified(
        string workflowId,
        [FromBody] RecordEligibilityVerifiedRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        ArgumentNullException.ThrowIfNull(request);
        await client.RecordEligibilityVerifiedAsync(workflowId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Records financing committed.</summary>
    /// <param name="workflowId">The ID of the purchase workflow.</param>
    /// <param name="request">The financing commitment details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content.</returns>
    [HttpPost("{workflowId}/financing")]
    [ProducesResponseType(204)]
    [EndpointName("recordFinancingCommitted")]
    public async Task<IActionResult> RecordFinancingCommitted(
        string workflowId,
        [FromBody] RecordFinancingCommittedRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        ArgumentNullException.ThrowIfNull(request);
        await client.RecordFinancingCommittedAsync(workflowId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Records inspection completed.</summary>
    /// <param name="workflowId">The ID of the purchase workflow.</param>
    /// <param name="request">The inspection completion details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content.</returns>
    [HttpPost("{workflowId}/inspection")]
    [ProducesResponseType(204)]
    [EndpointName("recordInspectionCompleted")]
    public async Task<IActionResult> RecordInspectionCompleted(
        string workflowId,
        [FromBody] RecordInspectionCompletedRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        ArgumentNullException.ThrowIfNull(request);
        await client.RecordInspectionCompletedAsync(workflowId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Records title cleared.</summary>
    /// <param name="workflowId">The ID of the purchase workflow.</param>
    /// <param name="request">The title clearance details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content.</returns>
    [HttpPost("{workflowId}/title")]
    [ProducesResponseType(204)]
    [EndpointName("recordTitleCleared")]
    public async Task<IActionResult> RecordTitleCleared(
        string workflowId,
        [FromBody] RecordTitleClearedRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        ArgumentNullException.ThrowIfNull(request);
        await client.RecordTitleClearedAsync(workflowId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Records lease executed.</summary>
    /// <param name="workflowId">The ID of the purchase workflow.</param>
    /// <param name="request">The lease execution details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content.</returns>
    [HttpPost("{workflowId}/lease-execution")]
    [ProducesResponseType(204)]
    [EndpointName("recordLeaseExecuted")]
    public async Task<IActionResult> RecordLeaseExecuted(
        string workflowId,
        [FromBody] RecordLeaseExecutedRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        ArgumentNullException.ThrowIfNull(request);
        await client.RecordLeaseExecutedAsync(workflowId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Schedules the closing date.</summary>
    /// <param name="workflowId">The ID of the purchase workflow.</param>
    /// <param name="request">The scheduled closing date.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content.</returns>
    [HttpPost("{workflowId}/closing/schedule")]
    [ProducesResponseType(204)]
    [EndpointName("scheduleClosing")]
    public async Task<IActionResult> ScheduleClosing(
        string workflowId,
        [FromBody] ScheduleClosingRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        ArgumentNullException.ThrowIfNull(request);
        await client.ScheduleClosingAsync(workflowId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Records the purchase closed.</summary>
    /// <param name="workflowId">The ID of the purchase workflow.</param>
    /// <param name="request">The closing details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content.</returns>
    [HttpPost("{workflowId}/closing/record")]
    [ProducesResponseType(204)]
    [EndpointName("recordPurchaseClosed")]
    public async Task<IActionResult> RecordPurchaseClosed(
        string workflowId,
        [FromBody] RecordPurchaseClosedRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        ArgumentNullException.ThrowIfNull(request);
        await client.RecordPurchaseClosedAsync(workflowId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Fails the purchase.</summary>
    /// <param name="workflowId">The ID of the purchase workflow.</param>
    /// <param name="request">The failure details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content.</returns>
    [HttpPost("{workflowId}/fail")]
    [ProducesResponseType(204)]
    [EndpointName("failPurchase")]
    public async Task<IActionResult> FailPurchase(
        string workflowId,
        [FromBody] FailPurchaseRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        ArgumentNullException.ThrowIfNull(request);
        await client.FailPurchaseAsync(workflowId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Withdraws the purchase.</summary>
    /// <param name="workflowId">The ID of the purchase workflow.</param>
    /// <param name="request">The withdrawal details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content.</returns>
    [HttpPost("{workflowId}/withdraw")]
    [ProducesResponseType(204)]
    [EndpointName("withdrawPurchase")]
    public async Task<IActionResult> WithdrawPurchase(
        string workflowId,
        [FromBody] WithdrawPurchaseRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        ArgumentNullException.ThrowIfNull(request);
        await client.WithdrawPurchaseAsync(workflowId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Stalls the workflow.</summary>
    /// <param name="workflowId">The ID of the purchase workflow.</param>
    /// <param name="request">The stall details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>204 No Content.</returns>
    [HttpPost("{workflowId}/stall")]
    [ProducesResponseType(204)]
    [EndpointName("stallWorkflow")]
    public async Task<IActionResult> StallWorkflow(
        string workflowId,
        [FromBody] StallWorkflowRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        ArgumentNullException.ThrowIfNull(request);
        await client.StallWorkflowAsync(workflowId, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }
}
