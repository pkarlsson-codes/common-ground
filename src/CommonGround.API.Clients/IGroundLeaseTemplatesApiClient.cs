using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.SharedKernel.Infrastructure.API;

namespace CommonGround.API.Clients;

/// <summary>Typed HTTP client for the ground lease template endpoints.</summary>
public interface IGroundLeaseTemplatesApiClient
{
    /// <summary>Gets a paginated list of ground lease templates.</summary>
    /// <param name="querySpec">The query parameters for pagination and filtering.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A paginated list of ground lease template summaries.</returns>
    Task<PaginatedResponse<GroundLeaseTemplateListItemResponse>> GetGroundLeaseTemplatesAsync(GetGroundLeaseTemplatesQuerySpec querySpec, CancellationToken cancellationToken);

    /// <summary>Creates a new ground lease template.</summary>
    /// <param name="request">The details of the template to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The ID of the newly created ground lease template.</returns>
    Task<CreateGroundLeaseTemplateResponse> CreateGroundLeaseTemplateAsync(CreateGroundLeaseTemplateRequest request, CancellationToken cancellationToken);
}
