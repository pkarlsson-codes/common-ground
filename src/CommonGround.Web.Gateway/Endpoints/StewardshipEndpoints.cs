using CommonGround.API.Clients;
using CommonGround.API.Contracts.Stewardship.Requests;
using CommonGround.API.Contracts.Stewardship.Responses;
using Wolverine.Http;

#pragma warning disable CA2007
#pragma warning disable SA1600

namespace CommonGround.Web.Gateway.Endpoints;

public static class StewardshipEndpoints
{
    // Stewardship Cases
    [WolverineGet("/api/v1/stewardship/cases")]
    public static async Task<IResult> GetAllStewardshipCases(
        IStewardshipApiClient client,
        CancellationToken cancellationToken)
    {
        var result = await client.GetAllStewardshipCasesAsync(cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverineGet("/api/v1/stewardship/cases/by-ground-lease/{groundLeaseId}")]
    public static async Task<IResult> GetStewardshipCasesByGroundLease(
        string groundLeaseId,
        IStewardshipApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        var result = await client.GetStewardshipCasesByGroundLeaseAsync(groundLeaseId, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverineGet("/api/v1/stewardship/cases/{caseId}")]
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

    // Foreclosure Prevention Cases
    [WolverineGet("/api/v1/stewardship/foreclosure-prevention")]
    public static async Task<IResult> GetAllForeclosurePreventionCases(
        IStewardshipApiClient client,
        CancellationToken cancellationToken)
    {
        var result = await client.GetAllForeclosurePreventionCasesAsync(cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverineGet("/api/v1/stewardship/foreclosure-prevention/by-ground-lease/{groundLeaseId}")]
    public static async Task<IResult> GetForeclosurePreventionCasesByGroundLease(
        string groundLeaseId,
        IStewardshipApiClient client,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        var result = await client.GetForeclosurePreventionCasesByGroundLeaseAsync(groundLeaseId, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    [WolverinePost("/api/v1/stewardship/foreclosure-prevention")]
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
}
