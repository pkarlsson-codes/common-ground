using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.SharedKernel.Infrastructure.API;

namespace CommonGround.API.Clients;

/// <summary>Typed HTTP client for the Homeownership resale endpoints.</summary>
public interface IResalesApiClient
{
    /// <summary>Records that a lease's homeowner has given notice they intend to sell.</summary>
    /// <param name="groundLeaseId">The ID of the homeowner's ground lease.</param>
    /// <param name="request">The notice details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The response containing the new resale ID.</returns>
    Task<RecordIntentToSellResponse> RecordIntentToSellAsync(string groundLeaseId, RecordIntentToSellRequest request, CancellationToken cancellationToken);

    /// <summary>Records that a homeowner withdrew their notice to sell.</summary>
    /// <param name="resaleId">The ID of the resale.</param>
    /// <param name="request">The withdrawal details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task WithdrawIntentToSellAsync(string resaleId, WithdrawIntentToSellRequest request, CancellationToken cancellationToken);

    /// <summary>Gets a paginated list of resales.</summary>
    /// <param name="querySpec">The query parameters for pagination and filtering.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A paginated list of resales.</returns>
    Task<PaginatedResponse<ResaleResponse>> GetResalesAsync(GetResalesQuerySpec querySpec, CancellationToken cancellationToken);

    /// <summary>Gets one resale.</summary>
    /// <param name="resaleId">The ID of the resale.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The resale.</returns>
    Task<ResaleResponse> GetResaleAsync(string resaleId, CancellationToken cancellationToken);

    /// <summary>Works out the prices an appraisal would set, without recording it.</summary>
    /// <param name="resaleId">The ID of the resale.</param>
    /// <param name="request">The appraisal figures.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The formula price and the purchase option price.</returns>
    Task<ResalePriceResponse> PreviewResalePriceAsync(string resaleId, PreviewResalePriceRequest request, CancellationToken cancellationToken);

    /// <summary>Records the appraisal the lease requires after a notice to sell.</summary>
    /// <param name="resaleId">The ID of the resale.</param>
    /// <param name="request">The appraisal.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RecordResaleAppraisalAsync(string resaleId, RecordResaleAppraisalRequest request, CancellationToken cancellationToken);

    /// <summary>Records that the trust will buy the home at the purchase option price.</summary>
    /// <param name="resaleId">The ID of the resale.</param>
    /// <param name="request">The decision.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ExercisePurchaseOptionAsync(string resaleId, ExercisePurchaseOptionRequest request, CancellationToken cancellationToken);

    /// <summary>Records that the trust paid for and took the home.</summary>
    /// <param name="resaleId">The ID of the resale.</param>
    /// <param name="request">The closing.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RecordBuybackClosedAsync(string resaleId, RecordBuybackClosedRequest request, CancellationToken cancellationToken);

    /// <summary>Works out what the seller owes the trust as of a day, for the closing agent.</summary>
    /// <param name="resaleId">The ID of the resale.</param>
    /// <param name="goodThrough">The day the figures are worked out for.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The fees, interest and credit, and the total owed.</returns>
    Task<PayoffStatementResponse> GetPayoffStatementAsync(string resaleId, DateOnly goodThrough, CancellationToken cancellationToken);

    /// <summary>Records that a payoff statement was printed for the closing agent.</summary>
    /// <param name="resaleId">The ID of the resale.</param>
    /// <param name="request">The day the statement is good through.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RecordPayoffStatementAsync(string resaleId, RecordPayoffStatementRequest request, CancellationToken cancellationToken);
}
