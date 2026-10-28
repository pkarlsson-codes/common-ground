using CommonGround.API.Clients;
using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.PropertyRegistry.Requests;
using CommonGround.API.Contracts.Stewardship.Requests;
using CommonGround.API.Contracts.Stewardship.Responses;
using CommonGround.Management.Gateway.Models;
using CommonGround.SharedKernel.Infrastructure.API;
using CommonGround.SharedKernel.Infrastructure.Querying;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace CommonGround.Management.Gateway.Endpoints;

public static class StewardshipEndpoints
{
    // Stewardship Cases
    [WolverineGet("/api/v1/stewardship/cases")]
    [ProducesResponseType<PaginatedResponse<StewardshipCaseResponse>>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetStewardshipCases(
        [FromQuery] GetStewardshipCasesQuerySpec querySpec,
        IStewardshipApiClient client,
        CancellationToken cancellationToken = default)
    {
        var result = await client.GetStewardshipCasesAsync(querySpec, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverineGet("/api/v1/stewardship/cases/{caseId}")]
    [ProducesResponseType<StewardshipCaseResponse>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetStewardshipCase(
        string caseId,
        IStewardshipApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        var result = await client.GetStewardshipCaseAsync(caseId, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverinePost("/api/v1/stewardship/cases/{caseId}/occupancy-verifications")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> InitiateOccupancyVerification(
        string caseId,
        InitiateOccupancyVerificationRequest request,
        IStewardshipApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.InitiateOccupancyVerificationAsync(caseId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/stewardship/cases/{caseId}/occupancy-verifications/{verificationId}/submit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> SubmitOccupancyVerification(
        string caseId,
        Guid verificationId,
        SubmitOccupancyVerificationRequest request,
        IStewardshipApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.SubmitOccupancyVerificationAsync(caseId, verificationId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/stewardship/cases/{caseId}/occupancy-verifications/{verificationId}/verify")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> VerifyOccupancy(
        string caseId,
        Guid verificationId,
        VerifyOccupancyRequest request,
        IStewardshipApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.VerifyOccupancyAsync(caseId, verificationId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/stewardship/cases/{caseId}/occupancy-verifications/{verificationId}/fail")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> FailOccupancy(
        string caseId,
        Guid verificationId,
        FailOccupancyRequest request,
        IStewardshipApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.FailOccupancyAsync(caseId, verificationId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/stewardship/cases/{caseId}/occupancy-verifications/{verificationId}/overdue")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> MarkOccupancyVerificationOverdue(
        string caseId,
        Guid verificationId,
        MarkOccupancyVerificationOverdueRequest request,
        IStewardshipApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.MarkOccupancyVerificationOverdueAsync(caseId, verificationId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/stewardship/cases/{caseId}/inspections")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> SchedulePropertyInspection(
        string caseId,
        SchedulePropertyInspectionRequest request,
        IStewardshipApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.SchedulePropertyInspectionAsync(caseId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/stewardship/cases/{caseId}/inspections/{inspectionId}/complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> CompletePropertyInspection(
        string caseId,
        Guid inspectionId,
        CompletePropertyInspectionRequest request,
        IStewardshipApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.CompletePropertyInspectionAsync(caseId, inspectionId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/stewardship/cases/{caseId}/inspections/{inspectionId}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> CancelPropertyInspection(
        string caseId,
        Guid inspectionId,
        CancelPropertyInspectionRequest request,
        IStewardshipApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.CancelPropertyInspectionAsync(caseId, inspectionId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/stewardship/cases/{caseId}/violations")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> IdentifyViolation(
        string caseId,
        IdentifyViolationRequest request,
        IStewardshipApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.IdentifyViolationAsync(caseId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/stewardship/cases/{caseId}/violations/{violationId}/notice")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> IssueViolationNotice(
        string caseId,
        Guid violationId,
        IssueViolationNoticeRequest request,
        IStewardshipApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.IssueViolationNoticeAsync(caseId, violationId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/stewardship/cases/{caseId}/violations/{violationId}/cure")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> CureViolation(
        string caseId,
        Guid violationId,
        CureViolationRequest request,
        IStewardshipApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.CureViolationAsync(caseId, violationId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/stewardship/cases/{caseId}/violations/{violationId}/escalate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> EscalateViolation(
        string caseId,
        Guid violationId,
        EscalateViolationRequest request,
        IStewardshipApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.EscalateViolationAsync(caseId, violationId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/stewardship/cases/{caseId}/close")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> CloseStewardshipCase(
        string caseId,
        CloseStewardshipCaseRequest request,
        IStewardshipApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.CloseStewardshipCaseAsync(caseId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    // Queues: every open case, with the homeowner and address of its lease, for the Stewardship page to sort and page.
    [WolverineGet("/api/v1/stewardship/queue/cases")]
    [ProducesResponseType<IReadOnlyList<StewardshipQueueItemResponse>>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetStewardshipQueue(
        IStewardshipApiClient client,
        IGroundLeasesApiClient groundLeases,
        ILandParcelsApiClient landParcels,
        CancellationToken cancellationToken)
    {
        var cases = await FetchAllAsync(
            page => client.GetStewardshipCasesAsync(
                new GetStewardshipCasesQuerySpec(page, PaginationQuerySpecV2.MaxPageSize), cancellationToken)).ConfigureAwait(false);
        var subjects = await GetLeaseSubjectsAsync(groundLeases, landParcels, cancellationToken).ConfigureAwait(false);

        return Results.Ok(cases
            .Select(c =>
            {
                var subject = subjects.GetValueOrDefault(c.GroundLeaseId);
                return new StewardshipQueueItemResponse { StewardshipCase = c, HomeownerFullName = subject.HomeownerFullName, Address = subject.Address };
            })
            .ToList());
    }

    [WolverineGet("/api/v1/stewardship/queue/foreclosure-prevention")]
    [ProducesResponseType<IReadOnlyList<ForeclosurePreventionQueueItemResponse>>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetForeclosurePreventionQueue(
        IStewardshipApiClient client,
        IGroundLeasesApiClient groundLeases,
        ILandParcelsApiClient landParcels,
        CancellationToken cancellationToken)
    {
        var cases = await FetchAllAsync(
            page => client.GetForeclosurePreventionCasesAsync(
                new GetForeclosurePreventionCasesQuerySpec(page, PaginationQuerySpecV2.MaxPageSize), cancellationToken)).ConfigureAwait(false);
        var subjects = await GetLeaseSubjectsAsync(groundLeases, landParcels, cancellationToken).ConfigureAwait(false);

        return Results.Ok(cases
            .Select(c =>
            {
                var subject = subjects.GetValueOrDefault(c.GroundLeaseId);
                return new ForeclosurePreventionQueueItemResponse { ForeclosurePreventionCase = c, HomeownerFullName = subject.HomeownerFullName, Address = subject.Address };
            })
            .ToList());
    }

    // Foreclosure Prevention Cases
    [WolverineGet("/api/v1/stewardship/foreclosure-prevention")]
    [ProducesResponseType<PaginatedResponse<ForeclosurePreventionCaseResponse>>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetForeclosurePreventionCases(
        [FromQuery] GetForeclosurePreventionCasesQuerySpec querySpec,
        IStewardshipApiClient client,
        CancellationToken cancellationToken = default)
    {
        var result = await client.GetForeclosurePreventionCasesAsync(querySpec, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverinePost("/api/v1/stewardship/foreclosure-prevention")]
    [ProducesResponseType<OpenForeclosurePreventionCaseResponse>(StatusCodes.Status202Accepted)]
    public static async Task<IResult> OpenForeclosurePreventionCase(
        OpenForeclosurePreventionCaseRequest request,
        IStewardshipApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var result = await client.OpenForeclosurePreventionCaseAsync(request, cancellationToken).ConfigureAwait(false);
        return Results.Accepted($"/api/v1/stewardship/foreclosure-prevention/{result.Id}", result);
    }

    [WolverineGet("/api/v1/stewardship/foreclosure-prevention/{caseId}")]
    [ProducesResponseType<ForeclosurePreventionCaseResponse>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetForeclosurePreventionCase(
        string caseId,
        IStewardshipApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        var result = await client.GetForeclosurePreventionCaseAsync(caseId, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverinePost("/api/v1/stewardship/foreclosure-prevention/{caseId}/servicer-delinquency")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> RecordServicerDelinquency(
        string caseId,
        RecordServicerDelinquencyRequest request,
        IStewardshipApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.RecordServicerDelinquencyAsync(caseId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/stewardship/foreclosure-prevention/{caseId}/counseling-referrals")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> MakeCounselingReferral(
        string caseId,
        MakeCounselingReferralRequest request,
        IStewardshipApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.MakeCounselingReferralAsync(caseId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/stewardship/foreclosure-prevention/{caseId}/counseling-referrals/{referralId}/outcome")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> RecordCounselingOutcome(
        string caseId,
        Guid referralId,
        RecordCounselingOutcomeRequest request,
        IStewardshipApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.RecordCounselingOutcomeAsync(caseId, referralId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/stewardship/foreclosure-prevention/{caseId}/forbearance-requests")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> RequestForbearance(
        string caseId,
        RequestForbearanceRequest request,
        IStewardshipApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.RequestForbearanceAsync(caseId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/stewardship/foreclosure-prevention/{caseId}/forbearance-requests/{requestId}/outcome")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> RecordForbearanceOutcome(
        string caseId,
        Guid requestId,
        RecordForbearanceOutcomeRequest request,
        IStewardshipApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.RecordForbearanceOutcomeAsync(caseId, requestId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/stewardship/foreclosure-prevention/{caseId}/cure-period")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> ActivateCurePeriod(
        string caseId,
        ActivateCurePeriodRequest request,
        IStewardshipApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.ActivateCurePeriodAsync(caseId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    [WolverinePost("/api/v1/stewardship/foreclosure-prevention/{caseId}/resolve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public static async Task<IResult> ResolveForeclosurePreventionCase(
        string caseId,
        ResolveForeclosurePreventionCaseRequest request,
        IStewardshipApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentNullException.ThrowIfNull(request);
        await client.ResolveForeclosurePreventionCaseAsync(caseId, request, cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    private static async Task<List<T>> FetchAllAsync<T>(Func<int, Task<PaginatedResponse<T>>> fetchPage)
    {
        var items = new List<T>();
        for (var page = 1; ; page++)
        {
            var result = await fetchPage(page).ConfigureAwait(false);
            items.AddRange(result.Items);
            if (!result.HasNextPage)
            {
                return items;
            }
        }
    }

    /// <summary>Homeowner and parcel address for every ground lease, active or not, keyed by lease ID.</summary>
    private static async Task<Dictionary<string, (string? HomeownerFullName, string? Address)>> GetLeaseSubjectsAsync(
        IGroundLeasesApiClient groundLeases,
        ILandParcelsApiClient landParcels,
        CancellationToken cancellationToken)
    {
        var leasesTask = FetchAllAsync(
            page => groundLeases.GetGroundLeasesAsync(
                new GetGroundLeasesQuerySpec(page, PaginationQuerySpecV2.MaxPageSize), cancellationToken));
        var parcelsTask = FetchAllAsync(
            page => landParcels.GetLandParcelsAsync(
                new GetLandParcelsQuerySpec(page, PaginationQuerySpecV2.MaxPageSize, includeDecommissioned: true), cancellationToken));
        await Task.WhenAll(leasesTask, parcelsTask).ConfigureAwait(false);

        var addresses = (await parcelsTask.ConfigureAwait(false)).ToDictionary(p => p.Id, p => p.Address);
        return (await leasesTask.ConfigureAwait(false)).ToDictionary(
            l => l.Id,
            l => (l.HomeownerFullName, addresses.GetValueOrDefault(l.LandParcelId)));
    }
}
