using CommonGround.API.Clients;
using CommonGround.API.Contracts.HomeBuyers.Requests;
using CommonGround.API.Contracts.HomeBuyers.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace CommonGround.Management.Gateway.Endpoints;

public static class HouseholdEndpoints
{
    [WolverinePost("/api/v1/households")]
    [ProducesResponseType<RegisterHouseholdResponse>(StatusCodes.Status201Created)]
    public static async Task<IResult> RegisterHousehold(
        RegisterHouseholdRequest request,
        IHouseholdsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var result = await client.RegisterHouseholdAsync(request, cancellationToken).ConfigureAwait(false);
        return Results.Created($"/api/v1/households/{result.HouseholdId}", result);
    }

    [WolverineGet("/api/v1/households")]
    [ProducesResponseType<PaginatedResponse<HouseholdListItemResponse>>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetHouseholds(
        [FromQuery] GetHouseholdsQuerySpec querySpec,
        IHouseholdsApiClient client,
        CancellationToken cancellationToken = default)
    {
        var result = await client.GetHouseholdsAsync(querySpec, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverineGet("/api/v1/households/{householdId}")]
    [ProducesResponseType<HouseholdResponse>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetHousehold(
        string householdId,
        IHouseholdsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(householdId);
        var result = await client.GetHouseholdAsync(householdId, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverinePost("/api/v1/households/{householdId}/members")]
    [ProducesResponseType<AddHouseholdMemberResponse>(StatusCodes.Status201Created)]
    public static async Task<IResult> AddHouseholdMember(
        string householdId,
        AddHouseholdMemberRequest request,
        IHouseholdsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(householdId);
        ArgumentNullException.ThrowIfNull(request);
        var result = await client.AddHouseholdMemberAsync(householdId, request, cancellationToken).ConfigureAwait(false);
        return Results.Created($"/api/v1/households/{householdId}", result);
    }

    [WolverinePut("/api/v1/households/{householdId}/members/{memberId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> UpdateHouseholdMember(
        string householdId,
        string memberId,
        UpdateHouseholdMemberRequest request,
        IHouseholdsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(householdId);
        ArgumentException.ThrowIfNullOrWhiteSpace(memberId);
        ArgumentNullException.ThrowIfNull(request);
        await client.UpdateHouseholdMemberAsync(householdId, memberId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverineDelete("/api/v1/households/{householdId}/members/{memberId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> RemoveHouseholdMember(
        string householdId,
        string memberId,
        IHouseholdsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(householdId);
        ArgumentException.ThrowIfNullOrWhiteSpace(memberId);
        await client.RemoveHouseholdMemberAsync(householdId, memberId, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePut("/api/v1/households/{householdId}/circumstances")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> UpdateHouseholdCircumstances(
        string householdId,
        UpdateHouseholdCircumstancesRequest request,
        IHouseholdsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(householdId);
        ArgumentNullException.ThrowIfNull(request);
        await client.UpdateHouseholdCircumstancesAsync(householdId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/households/{householdId}/housing-counseling")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> RecordHousingCounseling(
        string householdId,
        RecordHousingCounselingRequest request,
        IHouseholdsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(householdId);
        ArgumentNullException.ThrowIfNull(request);
        await client.RecordHousingCounselingAsync(householdId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/households/{householdId}/clt-orientation")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> RecordCltOrientation(
        string householdId,
        RecordCltOrientationRequest request,
        IHouseholdsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(householdId);
        ArgumentNullException.ThrowIfNull(request);
        await client.RecordCltOrientationAsync(householdId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/households/{householdId}/mortgage-prequalification")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> RecordMortgagePrequalification(
        string householdId,
        RecordMortgagePrequalificationRequest request,
        IHouseholdsApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(householdId);
        ArgumentNullException.ThrowIfNull(request);
        await client.RecordMortgagePrequalificationAsync(householdId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }
}
