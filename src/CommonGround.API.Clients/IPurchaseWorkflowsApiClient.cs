using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.SharedKernel.Infrastructure.API;

namespace CommonGround.API.Clients;

/// <summary>Typed HTTP client for the Homeownership purchase workflow endpoints.</summary>
public interface IPurchaseWorkflowsApiClient
{
    /// <summary>Initiates a new purchase workflow.</summary>
    /// <param name="request">The purchase workflow initiation details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The initiation response containing the new purchase workflow ID.</returns>
    Task<InitiatePurchaseWorkflowResponse> InitiatePurchaseWorkflowAsync(InitiatePurchaseWorkflowRequest request, CancellationToken cancellationToken);

    /// <summary>Gets a purchase workflow by its ID.</summary>
    /// <param name="workflowId">The ID of the purchase workflow to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The purchase workflow details.</returns>
    Task<PurchaseWorkflowResponse> GetPurchaseWorkflowAsync(string workflowId, CancellationToken cancellationToken);

    /// <summary>Gets a paginated list of purchase workflows.</summary>
    /// <param name="querySpec">The query specification containing pagination and filter parameters.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A paginated list of purchase workflow summaries.</returns>
    Task<PaginatedResponse<PurchaseWorkflowListItemResponse>> GetPurchaseWorkflowsAsync(GetPurchaseWorkflowsQuerySpec querySpec, CancellationToken cancellationToken);

    /// <summary>Gets every open purchase, grouped by the step it waits on.</summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The purchase board.</returns>
    Task<PurchaseBoardResponse> GetPurchaseBoardAsync(CancellationToken cancellationToken);

    /// <summary>Records buyer eligibility verified.</summary>
    /// <param name="workflowId">The ID of the purchase workflow.</param>
    /// <param name="request">The eligibility verification details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RecordEligibilityVerifiedAsync(string workflowId, RecordEligibilityVerifiedRequest request, CancellationToken cancellationToken);

    /// <summary>Records the appraisal completed.</summary>
    /// <param name="workflowId">The ID of the purchase workflow.</param>
    /// <param name="request">The appraisal details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RecordAppraisalCompletedAsync(string workflowId, RecordAppraisalCompletedRequest request, CancellationToken cancellationToken);

    /// <summary>Records financing committed.</summary>
    /// <param name="workflowId">The ID of the purchase workflow.</param>
    /// <param name="request">The financing commitment details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RecordFinancingCommittedAsync(string workflowId, RecordFinancingCommittedRequest request, CancellationToken cancellationToken);

    /// <summary>Records inspection completed.</summary>
    /// <param name="workflowId">The ID of the purchase workflow.</param>
    /// <param name="request">The inspection completion details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RecordInspectionCompletedAsync(string workflowId, RecordInspectionCompletedRequest request, CancellationToken cancellationToken);

    /// <summary>Records title cleared.</summary>
    /// <param name="workflowId">The ID of the purchase workflow.</param>
    /// <param name="request">The title clearance details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RecordTitleClearedAsync(string workflowId, RecordTitleClearedRequest request, CancellationToken cancellationToken);

    /// <summary>Records lease executed.</summary>
    /// <param name="workflowId">The ID of the purchase workflow.</param>
    /// <param name="request">The lease execution details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RecordLeaseExecutedAsync(string workflowId, RecordLeaseExecutedRequest request, CancellationToken cancellationToken);

    /// <summary>Schedules the closing date.</summary>
    /// <param name="workflowId">The ID of the purchase workflow.</param>
    /// <param name="request">The scheduled closing date.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ScheduleClosingAsync(string workflowId, ScheduleClosingRequest request, CancellationToken cancellationToken);

    /// <summary>Records the purchase closed.</summary>
    /// <param name="workflowId">The ID of the purchase workflow.</param>
    /// <param name="request">The closing details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RecordPurchaseClosedAsync(string workflowId, RecordPurchaseClosedRequest request, CancellationToken cancellationToken);

    /// <summary>Fails the purchase.</summary>
    /// <param name="workflowId">The ID of the purchase workflow.</param>
    /// <param name="request">The failure details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task FailPurchaseAsync(string workflowId, FailPurchaseRequest request, CancellationToken cancellationToken);

    /// <summary>Withdraws the purchase.</summary>
    /// <param name="workflowId">The ID of the purchase workflow.</param>
    /// <param name="request">The withdrawal details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task WithdrawPurchaseAsync(string workflowId, WithdrawPurchaseRequest request, CancellationToken cancellationToken);

    /// <summary>Stalls the workflow.</summary>
    /// <param name="workflowId">The ID of the purchase workflow.</param>
    /// <param name="request">The stall details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StallWorkflowAsync(string workflowId, StallWorkflowRequest request, CancellationToken cancellationToken);
}
