using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.SharedKernel.Infrastructure.API;

namespace CommonGround.API.Clients;

/// <summary>Typed HTTP client for the Homeownership ground lease endpoints.</summary>
public interface IGroundLeasesApiClient
{
    /// <summary>Creates a new ground lease.</summary>
    /// <param name="request">The ground lease creation details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The creation response containing the new ground lease ID.</returns>
    Task<CreateGroundLeaseResponse> CreateGroundLeaseAsync(CreateGroundLeaseRequest request, CancellationToken cancellationToken);

    /// <summary>Gets a ground lease by its ID.</summary>
    /// <param name="groundLeaseId">The ID of the ground lease to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The ground lease details.</returns>
    Task<GroundLeaseResponse> GetGroundLeaseAsync(string groundLeaseId, CancellationToken cancellationToken);

    /// <summary>Gets a paginated list of ground leases.</summary>
    /// <param name="querySpec">The query parameters for pagination and filtering.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A paginated list of ground lease summaries.</returns>
    Task<PaginatedResponse<GroundLeaseListItemResponse>> GetGroundLeasesAsync(GetGroundLeasesQuerySpec querySpec, CancellationToken cancellationToken);

    /// <summary>Records a stewardship check-in for a ground lease.</summary>
    /// <param name="groundLeaseId">The ID of the ground lease.</param>
    /// <param name="request">The check-in details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RecordStewardshipCheckInAsync(string groundLeaseId, RecordStewardshipCheckInRequest request, CancellationToken cancellationToken);

    /// <summary>Calculates a resale price estimate for a ground lease.</summary>
    /// <param name="groundLeaseId">The ID of the ground lease.</param>
    /// <param name="request">The calculation inputs.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The estimated maximum resale price.</returns>
    Task<ResaleEstimateResponse> CalculateResaleEstimateAsync(string groundLeaseId, CalculateResaleEstimateRequest request, CancellationToken cancellationToken);

    /// <summary>Records a lease fee payment the trust received on a lease.</summary>
    /// <param name="groundLeaseId">The ID of the ground lease.</param>
    /// <param name="request">The payment details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The response containing the new payment ID.</returns>
    Task<RecordLeaseFeePaymentResponse> RecordLeaseFeePaymentAsync(string groundLeaseId, RecordLeaseFeePaymentRequest request, CancellationToken cancellationToken);

    /// <summary>Voids a lease fee payment recorded by mistake.</summary>
    /// <param name="groundLeaseId">The ID of the ground lease.</param>
    /// <param name="paymentId">The ID of the payment.</param>
    /// <param name="request">Why it is voided.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task VoidLeaseFeePaymentAsync(string groundLeaseId, string paymentId, VoidLeaseFeePaymentRequest request, CancellationToken cancellationToken);

    /// <summary>Gets leases with what they owe in lease fees, most days overdue first.</summary>
    /// <param name="querySpec">The query parameters for pagination and filtering.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A paginated list of lease fee balances.</returns>
    Task<PaginatedResponse<LeaseFeeBalanceResponse>> GetLeaseFeeBalancesAsync(GetLeaseFeeBalancesQuerySpec querySpec, CancellationToken cancellationToken);

    /// <summary>Records that a late lease fee notice was sent on a lease.</summary>
    /// <param name="groundLeaseId">The ID of the ground lease.</param>
    /// <param name="request">The notice details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The response containing the new notice ID.</returns>
    Task<RecordLeaseFeeNoticeResponse> RecordLeaseFeeNoticeAsync(string groundLeaseId, RecordLeaseFeeNoticeRequest request, CancellationToken cancellationToken);

    /// <summary>Waives late-fee interest owed on a lease today.</summary>
    /// <param name="groundLeaseId">The ID of the ground lease.</param>
    /// <param name="request">The amount and reason.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The response containing the new waiver ID.</returns>
    Task<WaiveLeaseFeeInterestResponse> WaiveLeaseFeeInterestAsync(string groundLeaseId, WaiveLeaseFeeInterestRequest request, CancellationToken cancellationToken);

    /// <summary>Corrects the day a lease's fees are tracked from and what was owed on it.</summary>
    /// <param name="groundLeaseId">The ID of the ground lease.</param>
    /// <param name="request">The tracked-from day and opening balance.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that completes when the tracking is corrected.</returns>
    Task CorrectLeaseFeeTrackingAsync(string groundLeaseId, CorrectLeaseFeeTrackingRequest request, CancellationToken cancellationToken);

    /// <summary>Gets the late notices due, for printing; only one lease's when <paramref name="groundLeaseId"/> is given.</summary>
    /// <param name="groundLeaseId">The ID of one ground lease, or null for all.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The notices due, with the letter date and sign-off.</returns>
    Task<LeaseFeeNoticesDueResponse> GetLeaseFeeNoticesDueAsync(string? groundLeaseId, CancellationToken cancellationToken);

    /// <summary>Gets the trust's lease fee notice policy.</summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The policy, or the defaults.</returns>
    Task<LeaseFeeNoticePolicyResponse> GetLeaseFeeNoticePolicyAsync(CancellationToken cancellationToken);

    /// <summary>Sets the trust's lease fee notice policy and returns it.</summary>
    /// <param name="request">The new policy.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The policy as saved.</returns>
    Task<LeaseFeeNoticePolicyResponse> SetLeaseFeeNoticePolicyAsync(SetLeaseFeeNoticePolicyRequest request, CancellationToken cancellationToken);
}
