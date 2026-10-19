using CommonGround.API.Contracts.ApplicantSelection.Requests;
using CommonGround.API.Contracts.ApplicantSelection.Responses;
using CommonGround.SharedKernel.Infrastructure.API;

#pragma warning disable SA1611
#pragma warning disable SA1615

namespace CommonGround.API.Clients;

/// <summary>Typed HTTP client for Applicant Selection homeowner application endpoints.</summary>
public interface IApplicantSelectionApiClient
{
    /// <summary>Submits a new homeowner application.</summary>
    Task<SubmitApplicationResponse> SubmitApplicationAsync(SubmitApplicationRequest request, CancellationToken cancellationToken);

    /// <summary>Gets a paginated list of homeowner applications.</summary>
    Task<PaginatedResponse<HomeownerApplicationListItemResponse>> GetHomeownerApplicationsAsync(
        GetHomeownerApplicationsQuerySpec querySpec,
        CancellationToken cancellationToken);

    /// <summary>Gets a homeowner application by ID.</summary>
    Task<HomeownerApplicationResponse> GetHomeownerApplicationAsync(string applicationId, CancellationToken cancellationToken);

    /// <summary>Approves a homeowner application.</summary>
    Task ApproveApplicationAsync(string applicationId, ApproveEligibilityRequest request, CancellationToken cancellationToken);

    /// <summary>Gets applications eligible to be selected for a given home.</summary>
    Task<IReadOnlyList<HomeCandidateResponse>> GetHomeCandidatesAsync(string homeId, CancellationToken cancellationToken);

    /// <summary>Selects an applicant for a home.</summary>
    Task SelectApplicantAsync(string applicationId, SelectApplicantRequest request, CancellationToken cancellationToken);

    /// <summary>Denies a homeowner application.</summary>
    Task DenyApplicationAsync(string applicationId, DenyApplicationRequest request, CancellationToken cancellationToken);

    /// <summary>Withdraws a homeowner application.</summary>
    Task WithdrawApplicationAsync(string applicationId, WithdrawApplicationRequest request, CancellationToken cancellationToken);
}

#pragma warning restore SA1615
#pragma warning restore SA1611