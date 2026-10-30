using CommonGround.API.Clients;
using CommonGround.API.Contracts.HomeBuyers.Requests;
using CommonGround.API.Contracts.HomeBuyers.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace CommonGround.Management.Gateway.Endpoints;

public static class HomeownerApplicationsEndpoints
{
    [WolverinePost("/api/v1/homeowner-applications")]
    [ProducesResponseType<SubmitApplicationResponse>(StatusCodes.Status201Created)]
    public static async Task<IResult> SubmitApplication(
        SubmitApplicationRequest request,
        IHomeownerApplicationsApiClient client,
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
        IHomeownerApplicationsApiClient client,
        CancellationToken cancellationToken = default)
    {
        var result = await client.GetHomeownerApplicationsAsync(querySpec, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverineGet("/api/v1/homeowner-applications/{applicationId}")]
    [ProducesResponseType<HomeownerApplicationResponse>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetHomeownerApplication(
        string applicationId,
        IHomeownerApplicationsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        var result = await client.GetHomeownerApplicationAsync(applicationId, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverinePost("/api/v1/homeowner-applications/{applicationId}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> ApproveApplication(
        string applicationId,
        ApproveApplicationRequest request,
        IHomeownerApplicationsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        ArgumentNullException.ThrowIfNull(request);
        await client.ApproveApplicationAsync(applicationId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverineGet("/api/v1/homes/{homeId}/applicant-candidates")]
    [ProducesResponseType<IReadOnlyList<HomeCandidateResponse>>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetHomeCandidates(
        string homeId,
        IHomeownerApplicationsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);
        var result = await client.GetHomeCandidatesAsync(homeId, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverineGet("/api/v1/waiting-list")]
    [ProducesResponseType<PaginatedResponse<WaitingListEntryResponse>>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetWaitingList(
        [FromQuery] GetWaitingListQuerySpec querySpec,
        IHomeownerApplicationsApiClient client,
        CancellationToken cancellationToken = default)
    {
        var result = await client.GetWaitingListAsync(querySpec, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverinePost("/api/v1/homeowner-applications/{applicationId}/deny")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> DenyApplication(
        string applicationId,
        DenyApplicationRequest request,
        IHomeownerApplicationsApiClient client,
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
        IHomeownerApplicationsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        ArgumentNullException.ThrowIfNull(request);
        await client.WithdrawApplicationAsync(applicationId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }
}
