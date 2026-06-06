using CommonGround.API.Contracts.Stewardship.Requests;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.SharedKernel.Infrastructure.API;
using CommonGround.Stewardship.Application.Commands.StewardshipCases;
using CommonGround.Stewardship.Application.Queries;
using CommonGround.Stewardship.Domain.StewardshipCases;
using CommonGround.Stewardship.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace CommonGround.API.Stewardship;

#pragma warning disable SA1600
#pragma warning disable CA2007

public static class StewardshipCasesEndpoints
{
    [WolverineGet("/api/stewardship/cases/{id}")]
    public static async Task<IResult> GetById(string id, HttpContext httpContext, IMessageBus bus, CancellationToken ct)
    {
        var result = await bus.InvokeForTenantAsync<StewardshipCaseView?>(bus.TenantId!, new GetStewardshipCaseQuery(id), ct);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }

    [WolverineGet("/api/stewardship/cases")]
    public static async Task<IResult> GetByGroundLease([FromQuery] GetStewardshipCasesQuerySpec spec, HttpContext httpContext, IMessageBus bus, CancellationToken ct)
    {
        PaginatedResponse<StewardshipCaseView> result = string.IsNullOrWhiteSpace(spec.GroundLeaseId)
            ? await bus.InvokeForTenantAsync<PaginatedResponse<StewardshipCaseView>>(bus.TenantId!, new GetAllStewardshipCasesQuery(spec.Page), ct)
            : await bus.InvokeForTenantAsync<PaginatedResponse<StewardshipCaseView>>(
                bus.TenantId!, new GetStewardshipCasesByGroundLeaseQuery(spec.GroundLeaseId, spec.Page), ct);
        return Results.Ok(result);
    }

    [WolverinePost("/api/stewardship/cases/{id}/occupancy-verifications")]
    public static async Task<IResult> InitiateOccupancyVerification(string id, InitiateOccupancyVerificationRequest request, HttpContext httpContext, IMessageBus bus, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = httpContext.GetUserId();
        await bus.InvokeForTenantAsync(bus.TenantId!, new InitiateOccupancyVerificationCommand(
            id, request.PeriodYear, request.SurveyDeadline, userId), ct);
        return Results.NoContent();
    }

    [WolverinePost("/api/stewardship/cases/{id}/occupancy-verifications/{verificationId}/submit")]
    public static async Task<IResult> SubmitOccupancyVerification(string id, Guid verificationId, SubmitOccupancyVerificationRequest request, HttpContext httpContext, IMessageBus bus, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        await bus.InvokeForTenantAsync(bus.TenantId!, new SubmitOccupancyVerificationCommand(
            id, verificationId, request.IsPrimaryResident, request.SubletDisclosed,
            request.MajorChanges, request.SubmittedDate), ct);
        return Results.NoContent();
    }

    [WolverinePost("/api/stewardship/cases/{id}/occupancy-verifications/{verificationId}/verify")]
    public static async Task<IResult> VerifyOccupancy(string id, Guid verificationId, VerifyOccupancyRequest request, HttpContext httpContext, IMessageBus bus, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = httpContext.GetUserId();
        await bus.InvokeForTenantAsync(bus.TenantId!, new VerifyOccupancyCommand(
            id, verificationId, request.VerifiedDate, userId), ct);
        return Results.NoContent();
    }

    [WolverinePost("/api/stewardship/cases/{id}/occupancy-verifications/{verificationId}/fail")]
    public static async Task<IResult> FailOccupancy(string id, Guid verificationId, FailOccupancyRequest request, HttpContext httpContext, IMessageBus bus, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = httpContext.GetUserId();
        await bus.InvokeForTenantAsync(bus.TenantId!, new FailOccupancyCommand(
            id, verificationId, request.Reason, request.FailedDate, userId), ct);
        return Results.NoContent();
    }

    [WolverinePost("/api/stewardship/cases/{id}/occupancy-verifications/{verificationId}/overdue")]
    public static async Task<IResult> MarkOccupancyVerificationOverdue(string id, Guid verificationId, MarkOccupancyVerificationOverdueRequest request, HttpContext httpContext, IMessageBus bus, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        await bus.InvokeForTenantAsync(bus.TenantId!, new MarkOccupancyVerificationOverdueCommand(
            id, verificationId, request.Deadline), ct);
        return Results.NoContent();
    }

    [WolverinePost("/api/stewardship/cases/{id}/inspections")]
    public static async Task<IResult> SchedulePropertyInspection(string id, SchedulePropertyInspectionRequest request, HttpContext httpContext, IMessageBus bus, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = httpContext.GetUserId();
        await bus.InvokeForTenantAsync(bus.TenantId!, new SchedulePropertyInspectionCommand(
            id, request.ScheduledDate, request.InspectorName, userId), ct);
        return Results.NoContent();
    }

    [WolverinePost("/api/stewardship/cases/{id}/inspections/{inspectionId}/complete")]
    public static async Task<IResult> CompletePropertyInspection(string id, Guid inspectionId, CompletePropertyInspectionRequest request, HttpContext httpContext, IMessageBus bus, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = httpContext.GetUserId();
        await bus.InvokeForTenantAsync(bus.TenantId!, new CompletePropertyInspectionCommand(
            id, inspectionId, request.OccupancyConfirmed, request.ConditionNotes,
            request.CompletedDate, userId), ct);
        return Results.NoContent();
    }

    [WolverinePost("/api/stewardship/cases/{id}/inspections/{inspectionId}/cancel")]
    public static async Task<IResult> CancelPropertyInspection(string id, Guid inspectionId, CancelPropertyInspectionRequest request, HttpContext httpContext, IMessageBus bus, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = httpContext.GetUserId();
        await bus.InvokeForTenantAsync(bus.TenantId!, new CancelPropertyInspectionCommand(
            id, inspectionId, request.Reason, userId), ct);
        return Results.NoContent();
    }

    [WolverinePost("/api/stewardship/cases/{id}/violations")]
    public static async Task<IResult> IdentifyViolation(string id, IdentifyViolationRequest request, HttpContext httpContext, IMessageBus bus, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = httpContext.GetUserId();
        var violationType = LeaseViolationType.FromDisplayName(request.ViolationType);
        await bus.InvokeForTenantAsync(bus.TenantId!, new IdentifyViolationCommand(
            id, violationType, request.Description, request.IdentifiedDate, userId), ct);
        return Results.NoContent();
    }

    [WolverinePost("/api/stewardship/cases/{id}/violations/{violationId}/notice")]
    public static async Task<IResult> IssueViolationNotice(string id, Guid violationId, IssueViolationNoticeRequest request, HttpContext httpContext, IMessageBus bus, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = httpContext.GetUserId();
        await bus.InvokeForTenantAsync(bus.TenantId!, new IssueViolationNoticeCommand(
            id, violationId, request.NoticeDate, request.CureDeadline, userId), ct);
        return Results.NoContent();
    }

    [WolverinePost("/api/stewardship/cases/{id}/violations/{violationId}/cure")]
    public static async Task<IResult> CureViolation(string id, Guid violationId, CureViolationRequest request, HttpContext httpContext, IMessageBus bus, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = httpContext.GetUserId();
        await bus.InvokeForTenantAsync(bus.TenantId!, new CureViolationCommand(
            id, violationId, request.CuredDate, userId), ct);
        return Results.NoContent();
    }

    [WolverinePost("/api/stewardship/cases/{id}/violations/{violationId}/escalate")]
    public static async Task<IResult> EscalateViolation(string id, Guid violationId, EscalateViolationRequest request, HttpContext httpContext, IMessageBus bus, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = httpContext.GetUserId();
        await bus.InvokeForTenantAsync(bus.TenantId!, new EscalateViolationCommand(
            id, violationId, request.Reason, request.EscalatedDate, userId), ct);
        return Results.NoContent();
    }

    [WolverinePost("/api/stewardship/cases/{id}/close")]
    public static async Task<IResult> CloseStewardshipCase(string id, CloseStewardshipCaseRequest request, HttpContext httpContext, IMessageBus bus, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = httpContext.GetUserId();
        await bus.InvokeForTenantAsync(bus.TenantId!, new CloseStewardshipCaseCommand(
            id, request.Reason, request.ClosedDate, userId), ct);
        return Results.NoContent();
    }
}

#pragma warning restore CA2007
#pragma warning restore SA1600