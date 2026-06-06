using CommonGround.API.Clients;
using CommonGround.API.Contracts.ApplicantSelection.Requests;
using CommonGround.API.Contracts.ApplicantSelection.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace CommonGround.Web.Gateway.Endpoints;

/// <summary>Applicant selection endpoints.</summary>
public static class ApplicantSelectionEndpoints
{
    /// <summary>
    /// Submit application endpoint.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="client">Api client.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Response.</returns>
    [WolverinePost("/api/v1/homeowner-applications")]
    [ProducesResponseType<SubmitApplicationResponse>(StatusCodes.Status201Created)]
    public static async Task<IResult> SubmitApplication(
        SubmitApplicationRequest request,
        IApplicantSelectionApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var result = await client.SubmitApplicationAsync(request, cancellationToken).ConfigureAwait(false);
        return Results.Created($"/api/v1/homeowner-applications/{result.ApplicationId}", result);
    }

    /// <summary>
    /// Get homeowner applications endpoint.
    /// </summary>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="phase">The application phase.</param>
    /// <param name="search">The search term.</param>
    /// <param name="client">Api client.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Response.</returns>
    [WolverineGet("/api/v1/homeowner-applications")]
    [ProducesResponseType<PaginatedResponse<HomeownerApplicationListItemResponse>>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetHomeownerApplications(
        int pageNumber,
        int pageSize,
        string? phase,
        string? search,
        IApplicantSelectionApiClient client,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber <= 0)
        {
            pageNumber = 1;
        }

        if (pageSize <= 0)
        {
            pageSize = 20;
        }

        var result = await client.GetHomeownerApplicationsAsync(pageNumber, pageSize, phase, search, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    /// <summary>
    /// Get homeowner application endpoint.
    /// </summary>
    /// <param name="applicationId">The application ID.</param>
    /// <param name="client">Api client.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Response.</returns>
    [WolverineGet("/api/v1/homeowner-applications/{applicationId}")]
    [ProducesResponseType<HomeownerApplicationResponse>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetHomeownerApplication(
        string applicationId,
        IApplicantSelectionApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        var result = await client.GetHomeownerApplicationAsync(applicationId, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    /// <summary>
    /// Approve eligibility endpoint.
    /// </summary>
    /// <param name="applicationId">The application ID.</param>
    /// <param name="request">The request object.</param>
    /// <param name="client">Api client.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Response.</returns>
    [WolverinePost("/api/v1/homeowner-applications/{applicationId}/eligibility/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> ApproveEligibility(
        string applicationId,
        ApproveEligibilityRequest request,
        IApplicantSelectionApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        ArgumentNullException.ThrowIfNull(request);
        await client.ApproveEligibilityAsync(applicationId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    /// <summary>
    /// Deny eligibility endpoint.
    /// </summary>
    /// <param name="applicationId">The application ID.</param>
    /// <param name="request">The request object.</param>
    /// <param name="client">Api client.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Response.</returns>
    [WolverinePost("/api/v1/homeowner-applications/{applicationId}/eligibility/deny")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> DenyEligibility(
        string applicationId,
        DenyEligibilityRequest request,
        IApplicantSelectionApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        ArgumentNullException.ThrowIfNull(request);
        await client.DenyEligibilityAsync(applicationId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    /// <summary>
    /// Record housing counseling endpoint.
    /// </summary>
    /// <param name="applicationId">The application ID.</param>
    /// <param name="request">The request object.</param>
    /// <param name="client">Api client.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Response.</returns>
    [WolverinePost("/api/v1/homeowner-applications/{applicationId}/housing-counseling")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> RecordHousingCounseling(
        string applicationId,
        RecordHousingCounselingRequest request,
        IApplicantSelectionApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        ArgumentNullException.ThrowIfNull(request);
        await client.RecordHousingCounselingAsync(applicationId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    /// <summary>
    /// Record CLT orientation endpoint.
    /// </summary>
    /// <param name="applicationId">The application ID.</param>
    /// <param name="request">The request object.</param>
    /// <param name="client">Api client.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Response.</returns>
    [WolverinePost("/api/v1/homeowner-applications/{applicationId}/clt-orientation")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> RecordCltOrientation(
        string applicationId,
        RecordCltOrientationRequest request,
        IApplicantSelectionApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        ArgumentNullException.ThrowIfNull(request);
        await client.RecordCltOrientationAsync(applicationId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    /// <summary>
    /// Record mortgage prequalification endpoint.
    /// </summary>
    /// <param name="applicationId">The application ID.</param>
    /// <param name="request">The request object.</param>
    /// <param name="client">Api client.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Response.</returns>
    [WolverinePost("/api/v1/homeowner-applications/{applicationId}/mortgage-prequalification")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> RecordMortgagePrequalification(
        string applicationId,
        RecordMortgagePrequalificationRequest request,
        IApplicantSelectionApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        ArgumentNullException.ThrowIfNull(request);
        await client.RecordMortgagePrequalificationAsync(applicationId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    /// <summary>
    /// Select applicant endpoint.
    /// </summary>
    /// <param name="applicationId">The application ID.</param>
    /// <param name="request">The request object.</param>
    /// <param name="client">Api client.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Response.</returns>
    [WolverinePost("/api/v1/homeowner-applications/{applicationId}/select")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> SelectApplicant(
        string applicationId,
        SelectApplicantRequest request,
        IApplicantSelectionApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        ArgumentNullException.ThrowIfNull(request);
        await client.SelectApplicantAsync(applicationId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    /// <summary>
    /// Deny application endpoint.
    /// </summary>
    /// <param name="applicationId">The application ID.</param>
    /// <param name="request">The request object.</param>
    /// <param name="client">Api client.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Response.</returns>
    [WolverinePost("/api/v1/homeowner-applications/{applicationId}/deny")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> DenyApplication(
        string applicationId,
        DenyApplicationRequest request,
        IApplicantSelectionApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        ArgumentNullException.ThrowIfNull(request);
        await client.DenyApplicationAsync(applicationId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }
}
