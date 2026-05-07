using CommonGround.API.Clients;
using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

#pragma warning disable CA2007
#pragma warning disable SA1600

namespace CommonGround.Web.Gateway.Endpoints;

public static class GroundLeasesEndpoints
{
    [WolverinePost("/api/v1/ground-leases")]
    [ProducesResponseType<CreateGroundLeaseResponse>(StatusCodes.Status201Created)]
    public static async Task<IResult> CreateGroundLease(
        CreateGroundLeaseRequest request,
        IGroundLeasesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var result = await client.CreateGroundLeaseAsync(request, cancellationToken).ConfigureAwait(false);
        return Results.Created($"/api/v1/ground-leases/{result.GroundLeaseId}", result);
    }

    [WolverineGet("/api/v1/ground-leases/{groundLeaseId}")]
    [ProducesResponseType<GroundLeaseResponse>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetGroundLease(
        string groundLeaseId,
        IGroundLeasesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        var result = await client.GetGroundLeaseAsync(groundLeaseId, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverineGet("/api/v1/ground-leases")]
    [ProducesResponseType<PaginatedResponse<GroundLeaseListItemResponse>>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetGroundLeases(
        int pageNumber,
        int pageSize,
        string? status,
        string? landParcelId,
        string? homeId,
        string? search,
        IGroundLeasesApiClient client,
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

        var result = await client.GetGroundLeasesAsync(pageNumber, pageSize, status, landParcelId, homeId, search, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverinePatch("/api/v1/ground-leases/{groundLeaseId}/homeowner")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> ChangeHomeowner(
        string groundLeaseId,
        ChangeHomeownerRequest request,
        IGroundLeasesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.ChangeHomeownerAsync(groundLeaseId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/ground-leases/{groundLeaseId}/stewardship-check-ins")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> RecordStewardshipCheckIn(
        string groundLeaseId,
        RecordStewardshipCheckInRequest request,
        IGroundLeasesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.RecordStewardshipCheckInAsync(groundLeaseId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/ground-leases/{groundLeaseId}/resale-estimate")]
    [ProducesResponseType<ResaleEstimateResponse>(StatusCodes.Status200OK)]
    public static async Task<IResult> CalculateResaleEstimate(
        string groundLeaseId,
        CalculateResaleEstimateRequest request,
        IGroundLeasesApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);
        var result = await client.CalculateResaleEstimateAsync(groundLeaseId, request, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }
}
