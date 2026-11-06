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
}
