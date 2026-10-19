using CommonGround.API.Clients;
using CommonGround.API.Contracts.ApplicantSelection.Requests;
using CommonGround.API.Contracts.ApplicantSelection.Responses;
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
