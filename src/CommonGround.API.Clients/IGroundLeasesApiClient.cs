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

    /// <summary>Records a mortgage the trust permitted outside CommonGround on a lease.</summary>
    /// <param name="groundLeaseId">The ID of the ground lease.</param>
    /// <param name="request">The mortgage details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The response containing the new mortgage ID.</returns>
    Task<RecordPermittedMortgageResponse> RecordPermittedMortgageAsync(string groundLeaseId, RecordPermittedMortgageRequest request, CancellationToken cancellationToken);

    /// <summary>Changes a permitted mortgage's lender and notice address.</summary>
    /// <param name="groundLeaseId">The ID of the ground lease.</param>
    /// <param name="mortgageId">The ID of the permitted mortgage.</param>
    /// <param name="request">The new lender and notice address.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that completes when the mortgage is changed.</returns>
    Task EditPermittedMortgageAsync(string groundLeaseId, string mortgageId, EditPermittedMortgageRequest request, CancellationToken cancellationToken);

    /// <summary>Marks a permitted mortgage paid off.</summary>
    /// <param name="groundLeaseId">The ID of the ground lease.</param>
    /// <param name="mortgageId">The ID of the permitted mortgage.</param>
    /// <param name="request">The day it was paid off.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that completes when the mortgage is marked paid off.</returns>
    Task MarkPermittedMortgagePaidOffAsync(string groundLeaseId, string mortgageId, MarkPermittedMortgagePaidOffRequest request, CancellationToken cancellationToken);

    /// <summary>Records a homeowner's request for a new loan on the home.</summary>
    /// <param name="groundLeaseId">The ID of the ground lease.</param>
    /// <param name="request">The request body.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The response.</returns>
    Task<ReceiveMortgageRequestResponse> ReceiveMortgageRequestAsync(string groundLeaseId, ReceiveMortgageRequestRequest request, CancellationToken cancellationToken);

    /// <summary>Gets a lease's mortgage requests.</summary>
    /// <param name="groundLeaseId">The ID of the ground lease.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The response.</returns>
    Task<IReadOnlyList<MortgageRequestResponse>> GetMortgageRequestsAsync(string groundLeaseId, CancellationToken cancellationToken);

    /// <summary>Gets a mortgage request with its debt check.</summary>
    /// <param name="mortgageRequestId">The ID of the mortgage request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The response.</returns>
    Task<MortgageRequestResponse> GetMortgageRequestAsync(string mortgageRequestId, CancellationToken cancellationToken);

    /// <summary>Records the loan's appraisal on a request waiting for a decision.</summary>
    /// <param name="mortgageRequestId">The ID of the mortgage request.</param>
    /// <param name="request">The request body.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that completes when it is recorded.</returns>
    Task RecordMortgageRequestAppraisalAsync(string mortgageRequestId, RecordMortgageRequestAppraisalRequest request, CancellationToken cancellationToken);

    /// <summary>Approves a mortgage request.</summary>
    /// <param name="mortgageRequestId">The ID of the mortgage request.</param>
    /// <param name="request">The request body.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that completes when it is recorded.</returns>
    Task ApproveMortgageRequestAsync(string mortgageRequestId, ApproveMortgageRequestRequest request, CancellationToken cancellationToken);

    /// <summary>Denies a mortgage request.</summary>
    /// <param name="mortgageRequestId">The ID of the mortgage request.</param>
    /// <param name="request">The request body.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that completes when it is recorded.</returns>
    Task DenyMortgageRequestAsync(string mortgageRequestId, DenyMortgageRequestRequest request, CancellationToken cancellationToken);

    /// <summary>Records that an approved loan closed.</summary>
    /// <param name="mortgageRequestId">The ID of the mortgage request.</param>
    /// <param name="request">The request body.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The response.</returns>
    Task<RecordMortgageLoanClosedResponse> RecordMortgageLoanClosedAsync(string mortgageRequestId, RecordMortgageLoanClosedRequest request, CancellationToken cancellationToken);

    /// <summary>Withdraws a mortgage request.</summary>
    /// <param name="mortgageRequestId">The ID of the mortgage request.</param>
    /// <param name="request">The request body.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that completes when it is recorded.</returns>
    Task WithdrawMortgageRequestAsync(string mortgageRequestId, WithdrawMortgageRequestRequest request, CancellationToken cancellationToken);

    /// <summary>Works out a lease's resale price statement without recording it.</summary>
    /// <param name="groundLeaseId">The ID of the ground lease.</param>
    /// <param name="request">The statement date and the figures the formula needs.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The price and the figures used.</returns>
    Task<ResalePriceStatementPriceResponse> PreviewResalePriceStatementAsync(string groundLeaseId, PreviewResalePriceStatementRequest request, CancellationToken cancellationToken);

    /// <summary>Records that a resale price statement was sent on a lease.</summary>
    /// <param name="groundLeaseId">The ID of the ground lease.</param>
    /// <param name="request">The statement date, figures and delivery.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that completes when it is recorded.</returns>
    Task RecordResalePriceStatementAsync(string groundLeaseId, RecordResalePriceStatementRequest request, CancellationToken cancellationToken);
}
