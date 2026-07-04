using CommonGround.API.Clients;
using CommonGround.API.Contracts.ApplicantSelection.Requests;
using CommonGround.API.Contracts.ApplicantSelection.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace CommonGround.Management.Gateway.Endpoints;

public static class ApplicantSelectionEndpoints
{
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

    [WolverineGet("/api/v1/homeowner-applications")]
    [ProducesResponseType<PaginatedResponse<HomeownerApplicationListItemResponse>>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetHomeownerApplications(
        [FromQuery] GetHomeownerApplicationsQuerySpec querySpec,
        IApplicantSelectionApiClient client,
        CancellationToken cancellationToken = default)
    {
        var result = await client.GetHomeownerApplicationsAsync(querySpec, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

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

    [WolverineGet("/api/v1/homes/{homeId}/applicant-candidates")]
    [ProducesResponseType<IReadOnlyList<HomeCandidateResponse>>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetHomeCandidates(
        string homeId,
        [FromQuery] string? search,
        IApplicantSelectionApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);
        var result = await client.GetHomeCandidatesAsync(homeId, search, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

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

    [WolverinePost("/api/v1/homeowner-applications/{applicationId}/withdraw")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> WithdrawApplication(
        string applicationId,
        WithdrawApplicationRequest request,
        IApplicantSelectionApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        ArgumentNullException.ThrowIfNull(request);
        await client.WithdrawApplicationAsync(applicationId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }
}
