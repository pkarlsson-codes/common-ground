using System.Globalization;
using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.Homeownership.Application.Commands;
using CommonGround.Homeownership.Application.Queries;
using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.SharedKernel.Infrastructure.API;
using CommonGround.SharedKernel.Infrastructure.Querying;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CommonGround.API.Homeownership;

/// <summary>API controller for purchase workflow management.</summary>
/// <param name="bus">The Wolverine message bus.</param>
[ApiController]
[Route("api/purchase-workflows")]
public sealed class PurchaseWorkflowsController(IMessageBus bus) : ControllerBase
{
    /// <summary>Initiates a new purchase workflow.</summary>
    /// <param name="request">The request containing purchase workflow details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The ID of the newly initiated purchase workflow.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(InitiatePurchaseWorkflowResponse), 201)]
    [EndpointName("initiatePurchaseWorkflow")]
    public async Task<IActionResult> InitiatePurchaseWorkflow(
        [FromBody] InitiatePurchaseWorkflowRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var tenantId = HttpContext.GetTenantId();
        var userId = HttpContext.GetUserId();

        var command = new InitiatePurchaseWorkflow(
            GroundLeaseId.Create(request.GroundLeaseId),
            new Homeowner(request.BuyerFullName, request.BuyerEmail, request.BuyerPhone),
            request.PurchasePrice,
            userId);

        var id = await bus.InvokeForTenantAsync<PurchaseWorkflowId>(tenantId, command, cancellationToken);

        return CreatedAtAction(nameof(GetPurchaseWorkflow), new { workflowId = id.Value }, new InitiatePurchaseWorkflowResponse(id.Value));
    }

    /// <summary>Gets a paginated list of purchase workflows.</summary>
    /// <param name="request">The pagination and filter parameters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated list of purchase workflows.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<PurchaseWorkflowListItemResponse>), 200)]
    [EndpointName("getPurchaseWorkflows")]
    public async Task<IActionResult> GetPurchaseWorkflows(
        [FromQuery] PaginationQuerySpec<GetPurchaseWorkflowsFilter> request,
        CancellationToken cancellationToken = default)
    {
        var groundLeaseFilter = request.Filter?.GroundLeaseId is not null
            ? GroundLeaseId.Create(request.Filter.GroundLeaseId)
            : null;

        var query = new GetPurchaseWorkflows(request.Page, groundLeaseFilter, request.Filter?.Phase, request.Filter?.Search);

        var tenantId = HttpContext.GetTenantId();
        var result = await bus.InvokeForTenantAsync<PaginatedResponse<PurchaseWorkflowListItemView>>(tenantId, query, cancellationToken);

        var response = result.MapItems(v => new PurchaseWorkflowListItemResponse(
            v.Id,
            v.GroundLeaseId,
            v.BuyerFullName,
            v.PurchasePrice,
            v.Phase,
            v.CreatedAt.ToString("O", CultureInfo.InvariantCulture)));

        return Ok(response);
    }

    /// <summary>Gets a purchase workflow by ID.</summary>
    /// <param name="workflowId">The ID of the purchase workflow to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The detailed information of the purchase workflow.</returns>
    [HttpGet("{workflowId}")]
    [ProducesResponseType(typeof(PurchaseWorkflowResponse), 200)]
    [ProducesResponseType(404)]
    [EndpointName("getPurchaseWorkflow")]
    public async Task<IActionResult> GetPurchaseWorkflow(
        string workflowId,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);

        var tenantId = HttpContext.GetTenantId();
        var view = await bus.InvokeForTenantAsync<PurchaseWorkflowView>(
            tenantId,
            new GetPurchaseWorkflow(PurchaseWorkflowId.Create(workflowId)),
            cancellationToken);

        var response = MapToResponse(view);

        return Ok(response);
    }

    /// <summary>Records buyer eligibility verified.</summary>
    /// <param name="workflowId">The ID of the purchase workflow.</param>
    /// <param name="request">The request containing eligibility verification details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
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

        var tenantId = HttpContext.GetTenantId();
        var userId = HttpContext.GetUserId();

        await bus.InvokeForTenantAsync(
            tenantId,
            new RecordEligibilityVerified(
                PurchaseWorkflowId.Create(workflowId),
                DateOnly.Parse(request.VerifiedDate, CultureInfo.InvariantCulture),
                request.Notes,
                userId),
            cancellationToken);

        return NoContent();
    }

    /// <summary>Records financing committed.</summary>
    /// <param name="workflowId">The ID of the purchase workflow.</param>
    /// <param name="request">The request containing financing details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
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

        var tenantId = HttpContext.GetTenantId();
        var userId = HttpContext.GetUserId();

        await bus.InvokeForTenantAsync(
            tenantId,
            new RecordFinancingCommitted(
                PurchaseWorkflowId.Create(workflowId),
                request.LenderName,
                request.CommittedAmount,
                DateOnly.Parse(request.PreapprovalLetterDate, CultureInfo.InvariantCulture),
                userId),
            cancellationToken);

        return NoContent();
    }

    /// <summary>Records inspection completed.</summary>
    /// <param name="workflowId">The ID of the purchase workflow.</param>
    /// <param name="request">The request containing inspection details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
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

        var tenantId = HttpContext.GetTenantId();
        var userId = HttpContext.GetUserId();

        await bus.InvokeForTenantAsync(
            tenantId,
            new RecordInspectionCompleted(
                PurchaseWorkflowId.Create(workflowId),
                DateOnly.Parse(request.InspectionDate, CultureInfo.InvariantCulture),
                request.InspectorName,
                request.Notes,
                userId),
            cancellationToken);

        return NoContent();
    }

    /// <summary>Records title cleared.</summary>
    /// <param name="workflowId">The ID of the purchase workflow.</param>
    /// <param name="request">The request containing title clearance details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
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

        var tenantId = HttpContext.GetTenantId();
        var userId = HttpContext.GetUserId();

        await bus.InvokeForTenantAsync(
            tenantId,
            new RecordTitleCleared(
                PurchaseWorkflowId.Create(workflowId),
                DateOnly.Parse(request.ClearedDate, CultureInfo.InvariantCulture),
                request.TitleCompanyName,
                userId),
            cancellationToken);

        return NoContent();
    }

    /// <summary>Records lease executed.</summary>
    /// <param name="workflowId">The ID of the purchase workflow.</param>
    /// <param name="request">The request containing lease execution details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
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

        var tenantId = HttpContext.GetTenantId();
        var userId = HttpContext.GetUserId();

        await bus.InvokeForTenantAsync(
            tenantId,
            new RecordLeaseExecuted(
                PurchaseWorkflowId.Create(workflowId),
                DateOnly.Parse(request.SignedDate, CultureInfo.InvariantCulture),
                userId),
            cancellationToken);

        return NoContent();
    }

    /// <summary>Schedules the closing date.</summary>
    /// <param name="workflowId">The ID of the purchase workflow.</param>
    /// <param name="request">The request containing the scheduled closing date.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
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

        var tenantId = HttpContext.GetTenantId();
        var userId = HttpContext.GetUserId();

        await bus.InvokeForTenantAsync(
            tenantId,
            new ScheduleClosing(
                PurchaseWorkflowId.Create(workflowId),
                DateOnly.Parse(request.ScheduledClosingDate, CultureInfo.InvariantCulture),
                userId),
            cancellationToken);

        return NoContent();
    }

    /// <summary>Records the purchase closed.</summary>
    /// <param name="workflowId">The ID of the purchase workflow.</param>
    /// <param name="request">The request containing closing details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
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

        var tenantId = HttpContext.GetTenantId();
        var userId = HttpContext.GetUserId();

        await bus.InvokeForTenantAsync(
            tenantId,
            new RecordPurchaseClosed(
                PurchaseWorkflowId.Create(workflowId),
                DateOnly.Parse(request.ActualClosingDate, CultureInfo.InvariantCulture),
                request.Notes,
                userId),
            cancellationToken);

        return NoContent();
    }

    /// <summary>Fails the purchase.</summary>
    /// <param name="workflowId">The ID of the purchase workflow.</param>
    /// <param name="request">The request containing failure details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
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

        var tenantId = HttpContext.GetTenantId();
        var userId = HttpContext.GetUserId();

        await bus.InvokeForTenantAsync(
            tenantId,
            new FailPurchase(
                PurchaseWorkflowId.Create(workflowId),
                request.FailureReason,
                DateOnly.Parse(request.FailedDate, CultureInfo.InvariantCulture),
                userId),
            cancellationToken);

        return NoContent();
    }

    /// <summary>Withdraws the purchase.</summary>
    /// <param name="workflowId">The ID of the purchase workflow.</param>
    /// <param name="request">The request containing withdrawal details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
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

        var tenantId = HttpContext.GetTenantId();
        var userId = HttpContext.GetUserId();

        await bus.InvokeForTenantAsync(
            tenantId,
            new WithdrawPurchase(
                PurchaseWorkflowId.Create(workflowId),
                DateOnly.Parse(request.WithdrawnDate, CultureInfo.InvariantCulture),
                request.Reason,
                userId),
            cancellationToken);

        return NoContent();
    }

    /// <summary>Stalls the workflow.</summary>
    /// <param name="workflowId">The ID of the purchase workflow.</param>
    /// <param name="request">The request containing stall details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
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

        var tenantId = HttpContext.GetTenantId();
        var userId = HttpContext.GetUserId();

        await bus.InvokeForTenantAsync(
            tenantId,
            new StallWorkflow(
                PurchaseWorkflowId.Create(workflowId),
                request.StalledReason,
                DateOnly.Parse(request.StalledDate, CultureInfo.InvariantCulture),
                DateOnly.Parse(request.ResolutionTargetDate, CultureInfo.InvariantCulture),
                userId),
            cancellationToken);

        return NoContent();
    }

    private static PurchaseWorkflowResponse MapToResponse(PurchaseWorkflowView view)
    {
        var eligibility = view.EligibilityDetails is null ? null
            : new EligibilityDetailsDto(view.EligibilityDetails.VerifiedDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), view.EligibilityDetails.Notes);
        var financing = view.FinancingDetails is null ? null
            : new FinancingDetailsDto(view.FinancingDetails.LenderName, view.FinancingDetails.CommittedAmount, view.FinancingDetails.PreapprovalLetterDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        var inspection = view.InspectionResult is null ? null
            : new InspectionResultDto(view.InspectionResult.InspectionDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), view.InspectionResult.InspectorName, view.InspectionResult.Notes);
        var title = view.TitleDetails is null ? null
            : new TitleDetailsDto(view.TitleDetails.ClearedDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), view.TitleDetails.TitleCompanyName);
        var leaseExecution = view.LeaseExecutionDetails is null ? null
            : new LeaseExecutionDetailsDto(view.LeaseExecutionDetails.SignedDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

        return new PurchaseWorkflowResponse(
            view.Id,
            view.GroundLeaseId,
            view.Phase,
            view.InitiatedBy,
            view.BuyerFullName,
            view.BuyerEmail,
            view.BuyerPhone,
            view.PurchasePrice,
            view.CreatedAt.ToString("O", CultureInfo.InvariantCulture),
            eligibility,
            financing,
            inspection,
            title,
            leaseExecution,
            view.ScheduledClosingDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            view.ActualClosingDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            view.ClosingNotes,
            view.FailureReason,
            view.WithdrawalReason,
            view.StalledReason,
            view.StalledDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            view.ResolutionTargetDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
    }
}
